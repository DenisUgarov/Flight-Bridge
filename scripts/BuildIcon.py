from pathlib import Path
import sys
from PIL import Image


def main() -> int:
    if len(sys.argv) != 4:
        print("usage: BuildIcon.py INPUT.png OUTPUT.png OUTPUT.ico", file=sys.stderr)
        return 2
    source = Path(sys.argv[1])
    png = Path(sys.argv[2])
    ico = Path(sys.argv[3])
    image = Image.open(source).convert("RGBA")
    image = image.resize((1024, 1024), Image.Resampling.LANCZOS)
    png.parent.mkdir(parents=True, exist_ok=True)
    image.save(png, "PNG", optimize=True)
    image.save(
        ico,
        "ICO",
        sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (40, 40),
               (48, 48), (64, 64), (96, 96), (128, 128), (256, 256)],
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
