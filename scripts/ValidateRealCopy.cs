using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FSMigrator;

// Developer-only validation. It clones the discovered Steam Cloud folders into
// %TEMP% and performs migration/restore there. The real game folders are read only.
internal static class ValidateRealCopy
{
    private static void CopyTree(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
        {
            string copy = Path.Combine(destination, Path.GetFileName(file));
            File.WriteAllBytes(copy, File.ReadAllBytes(file));
            File.SetAttributes(copy, FileAttributes.Normal);
        }
        foreach (var directory in Directory.GetDirectories(source))
            CopyTree(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

    private static Installation CloneStore(Installation original, string root)
    {
        var clone = new Installation
        {
            Year = original.Year,
            Edition = "Steam",
            Root = root,
            Account = "shadow",
            InstallPath = original.InstallPath
        };
        foreach (var file in Directory.GetFiles(Path.Combine(root, "remote"), "inputprofile_*"))
        {
            if (!Regex.IsMatch(Path.GetFileName(file), @"^inputprofile_(?:inputprofile_)?[0-9]+$"))
                continue;
            clone.Profiles.Add(Profile.Read(file));
        }
        return clone;
    }

    private static Dictionary<string, string> Hashes(params string[] roots)
    {
        return roots.SelectMany(AutoMigration.SafeFiles)
            .ToDictionary(path => path, Engine.Hash, StringComparer.OrdinalIgnoreCase);
    }

    private static bool SameHashes(Dictionary<string, string> before, Dictionary<string, string> after)
    {
        return before.Count == after.Count && before.All(pair =>
            after.ContainsKey(pair.Key) && after[pair.Key] == pair.Value);
    }

    private static int Main()
    {
        string work = Path.Combine(Environment.CurrentDirectory, ".validation", "real-shadow-" + Guid.NewGuid().ToString("N"));
        bool succeeded=false;
        try
        {
            // Use the oldest effective Flight Bridge snapshot as the pristine
            // baseline.  The currently installed profiles may already contain
            // output from an earlier test build, while the immutable snapshot
            // contains their native MSFS 2024 action metadata.
            string baselineManifest = MigrationTransaction.Sessions(MigrationTransaction.DefaultRoot)
                .Where(m => MigrationTransaction.State(m) == "Completed" && MigrationTransaction.HasEffectiveChanges(m))
                .Last();
            var baseline = MigrationTransaction.Verify(baselineManifest);
            string baselineFolder = Path.GetDirectoryName(baselineManifest);
            var baselineStores = baseline.Root.Elements("Store").Select(element => new {
                Year = (string)element.Attribute("Year"),
                Root = Path.Combine(baselineFolder, (string)element.Attribute("Copy"))
            }).ToList();
            var sourceReal = CloneStore(new Installation { Year="2020", Edition="Steam", InstallPath="baseline" }, baselineStores.Single(s=>s.Year=="2020").Root);
            sourceReal.Year="2020";
            var targetReal = CloneStore(new Installation { Year="2024", Edition="Steam", InstallPath="baseline" }, baselineStores.Single(s=>s.Year=="2024").Root);
            targetReal.Year="2024";
            var allTargetReal = targetReal.Profiles.Single(profile =>
                profile.Name.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                ((string)profile.Device.Attribute("DeviceName") ?? "").Equals("T-Rudder", StringComparison.OrdinalIgnoreCase));
            var candidates = AutoMigration.CompatibleSources(sourceReal, allTargetReal)
                .OrderBy(profile => profile.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

            Console.WriteLine("Real folders are read only. Verified pristine backup is the test baseline. Shadow candidates: " + candidates.Count);
            {
                string scenario = Path.Combine(work, "automatic-one-click");
                string sourceRoot = Path.Combine(scenario, "1250410");
                string targetRoot = Path.Combine(scenario, "2537590");
                CopyTree(sourceReal.Root, sourceRoot);
                CopyTree(targetReal.Root, targetRoot);
                var source = CloneStore(sourceReal, sourceRoot);
                var target = CloneStore(targetReal, targetRoot);
                var automatic = AutoMigration.Build(new List<Installation> { source, target });
                if (!automatic.Ready || automatic.Changes.Count != 3 || automatic.Notices.Count == 0)
                    throw new InvalidOperationException("The one-click plan did not select exactly the three safe R66 targets.");
                var before = Hashes(sourceRoot, targetRoot);
                string manifest = MigrationTransaction.Execute(automatic, Path.Combine(scenario, "backups"), () => { }, null);
                MigrationTransaction.Verify(manifest);
                MigrationTransaction.Restore(manifest, () => { });
                if (!SameHashes(before, Hashes(sourceRoot, targetRoot)))
                    throw new IOException("One-click restore did not return the shadow tree byte-for-byte.");
                Console.WriteLine("One-click plan: 3 safe profiles updated, ambiguous profile untouched, full restore verified");
                Directory.Delete(scenario, true);
            }
            foreach (var candidateReal in candidates)
            {
                string scenario = Path.Combine(work, Guid.NewGuid().ToString("N"));
                string sourceRoot = Path.Combine(scenario, "1250410");
                string targetRoot = Path.Combine(scenario, "2537590");
                CopyTree(sourceReal.Root, sourceRoot);
                CopyTree(targetReal.Root, targetRoot);
                var source = CloneStore(sourceReal, sourceRoot);
                var target = CloneStore(targetReal, targetRoot);
                var targetProfile = target.Profiles.Single(profile => Path.GetFileName(profile.Path) == Path.GetFileName(allTargetReal.Path));
                var sourceProfile = source.Profiles.Single(profile => Path.GetFileName(profile.Path) == Path.GetFileName(candidateReal.Path));
                var choices = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                choices[targetProfile.Path] = sourceProfile.Path;
                var plan = AutoMigration.Build(new List<Installation> { source, target }, choices);
                if (!plan.Ready)
                    throw new InvalidOperationException(candidateReal.Name + ": " + string.Join("; ", plan.Issues));
                var before = Hashes(sourceRoot, targetRoot);
                string manifest = MigrationTransaction.Execute(plan, Path.Combine(scenario, "backups"), () => { }, null);
                MigrationTransaction.Verify(manifest);
                foreach (var change in plan.Changes)
                    Profile.Read(change.Target.Path);
                MigrationTransaction.Restore(manifest, () => { });
                var after = Hashes(sourceRoot, targetRoot);
                if (!SameHashes(before, after))
                    throw new IOException("Restore did not return the shadow tree byte-for-byte: " + candidateReal.Name);
                var selected = plan.Changes.Single(change => Path.GetFileName(change.Target.Path) == Path.GetFileName(targetProfile.Path));
                Console.WriteLine(candidateReal.Name + ": copied=" + selected.Copied +
                    ", axes=" + selected.Axes + ", skipped=" + selected.Skipped.Count +
                    ", full backup verified, byte-exact restore verified");
                Directory.Delete(scenario, true);
            }
            Console.WriteLine("PASS: all real profile formats completed migration and restore on isolated copies.");
            succeeded=true;return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        finally
        {
            if (succeeded && Directory.Exists(work))
                Directory.Delete(work, true);
        }
    }
}
