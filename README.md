# ⚠️ Flight Bridge 0.5.1 — экспериментальный проект

> [!CAUTION]
> **Русский:** Программа экспериментальная и ещё не готова к использованию.
> Пока не устанавливайте и не запускайте её. Она изменяет файлы профилей
> контроллеров Microsoft Flight Simulator и может привести к повреждению или
> необратимой потере профилей, настроек и других пользовательских данных.
>
> **English:** This software is experimental and not ready for use. Do not install
> or run it yet. It modifies Microsoft Flight Simulator controller profile files
> and may cause corruption or irreversible loss of profiles, settings, or other
> user data.
>
> **Deutsch:** Diese Software ist experimentell und noch nicht einsatzbereit.
> Installieren oder starten Sie sie derzeit nicht. Sie verändert Controllerprofile
> von Microsoft Flight Simulator und kann Profile, Einstellungen oder andere
> Benutzerdaten beschädigen oder unwiederbringlich löschen.
>
> **Français :** Ce logiciel est expérimental et n'est pas encore prêt à être
> utilisé. Ne l'installez pas et ne l'exécutez pas pour le moment. Il modifie les
> profils de contrôleurs de Microsoft Flight Simulator et peut les endommager ou
> entraîner la perte irréversible de profils, de réglages ou d'autres données.
>
> **Español:** Este software es experimental y todavía no está listo para su uso.
> No lo instale ni lo ejecute por ahora. Modifica los perfiles de control de
> Microsoft Flight Simulator y puede dañarlos o provocar la pérdida irreversible
> de perfiles, ajustes u otros datos del usuario.
>
> **Italiano:** Questo software è sperimentale e non è ancora pronto per l'uso.
> Per ora non installarlo né eseguirlo. Modifica i profili dei controller di
> Microsoft Flight Simulator e può danneggiare o causare la perdita irreversibile
> di profili, impostazioni o altri dati dell'utente.
>
> **Português:** Este software é experimental e ainda não está pronto para uso.
> Não o instale nem execute por enquanto. Ele altera os perfis de controle do
> Microsoft Flight Simulator e pode danificá-los ou causar a perda irreversível de
> perfis, configurações ou outros dados do usuário.
>
> **Polski:** To oprogramowanie jest eksperymentalne i nie jest jeszcze gotowe do
> użycia. Na razie nie należy go instalować ani uruchamiać. Modyfikuje profile
> kontrolerów Microsoft Flight Simulator i może spowodować ich uszkodzenie lub
> nieodwracalną utratę profili, ustawień albo innych danych użytkownika.
>
> **Українська:** Програма експериментальна й ще не готова до використання. Поки
> що не встановлюйте та не запускайте її. Вона змінює файли профілів контролерів
> Microsoft Flight Simulator і може пошкодити або безповоротно видалити профілі,
> налаштування чи інші дані користувача.
>
> **Türkçe:** Bu yazılım deneyseldir ve henüz kullanıma hazır değildir. Şimdilik
> yüklemeyin veya çalıştırmayın. Microsoft Flight Simulator denetleyici profillerini
> değiştirir ve profillerin, ayarların ya da diğer kullanıcı verilerinin bozulmasına
> veya geri döndürülemez biçimde kaybolmasına neden olabilir.
>
> **简体中文：** 此软件仍处于实验阶段，尚未准备好投入使用。请暂时不要安装或运行。
> 它会修改 Microsoft Flight Simulator 的控制器配置文件，可能导致配置文件、设置或
> 其他用户数据损坏或永久丢失。
>
> **日本語：** このソフトウェアは実験段階であり、まだ使用できる状態ではありません。
> 現時点ではインストールまたは実行しないでください。Microsoft Flight Simulator の
> コントローラープロファイルを変更するため、プロファイル、設定、その他のユーザーデータが
> 破損したり、元に戻せない形で失われたりする可能性があります。
>
> **한국어:** 이 소프트웨어는 실험 단계이며 아직 사용할 준비가 되지 않았습니다. 현재는
> 설치하거나 실행하지 마십시오. Microsoft Flight Simulator 컨트롤러 프로필을 변경하므로
> 프로필, 설정 또는 기타 사용자 데이터가 손상되거나 복구할 수 없게 손실될 수 있습니다.
>
> The repository is currently published for source review and development only.

Flight Bridge автоматически переносит существующие настройки контроллеров из
Microsoft Flight Simulator 2020 в зарегистрированные профили Microsoft Flight
Simulator 2024.

Репозиторий содержит полный исходный код приложения и установщика, сценарий
сборки, автоматические тесты и безопасные синтетические примеры формата профилей.
Реальные игровые профили, резервные копии и собранные EXE в репозиторий не входят.

## Сборка из исходников

На Windows 10/11 откройте PowerShell в корне проекта и выполните:

```powershell
.\build.ps1
```

Сценарий компилирует приложение и установщик штатным C#-компилятором .NET
Framework, запускает все тесты и создаёт папку текущего выпуска в `dist`.

## Как пользоваться

1. Установите и запустите Flight Bridge. Программа сама найдёт обе игры,
   хранилища и совместимые профили.
2. Полностью закройте MSFS 2020, MSFS 2024 и Steam, если используется Steam.
3. Нажмите **«Перенести настройки»**.
4. Запустите MSFS 2024 и проверьте оси и кнопки перед полётом.

Если версия 0.5.0 уже выполняла перенос на этом компьютере, версия 0.5.1 сама
обнаружит его резервную копию. Одна кнопка сначала вернёт исходные профили, а
затем сразу выполнит исправленный перенос.

Выбирать папки, XML-файлы или технические параметры не требуется. Если профиль
нельзя сопоставить однозначно, он остаётся без изменений. Программа не угадывает
и не создаёт незарегистрированные облачные записи.

Установщик и приложение поддерживают 13 языков: английский, русский, немецкий,
французский, испанский, итальянский, португальский, польский, украинский,
турецкий, упрощённый китайский, японский и корейский. Язык Windows выбирается
автоматически. При необходимости его можно изменить в видимом списке языка в
правом верхнем углу окна.

## Резервная копия и возврат

Перед любой записью Flight Bridge обязательно копирует целиком исходное и
целевое облачное хранилище, вычисляет SHA-256 каждого файла и повторно проверяет,
что данные игры не изменились во время подготовки. Отключить бэкап нельзя.

Копии по умолчанию находятся в `Документы\Flight Bridge\Backups`. Место хранения
можно заранее изменить в свёрнутом разделе приложения, например выбрать внешний
диск.

Каждый целевой профиль заменяется атомарно. После записи файл повторно читается и
проверяется. При ошибке программа автоматически возвращает уже изменённые файлы.
После прерывания питания или завершения процесса следующий запуск потребует сначала
нажать **«Вернуть настройки из последней копии»**.

Восстановление проверяет всю копию и возвращает только файлы, изменённые конкретным
переносом. Если пользователь позднее изменил профиль в игре, автоматическое
восстановление остановится, чтобы не уничтожить новые настройки.

## Поддерживаемые хранилища

- Steam: MSFS 2020 (`1250410`) и MSFS 2024 (`2537590`), включая библиотеки на
  других дисках и активный аккаунт Steam.
- Microsoft Store/Xbox WGS: изменение только существующего файла профиля внутри
  зарегистрированного контейнера. `containers.index` и метаданные контейнера не
  создаются и не переписываются.
- Смешанная установка Steam/Microsoft Store поддерживается.

Полный перенос и восстановление проверены на изолированных копиях нескольких
реальных форматов профилей. Сами игровые хранилища в эти проверки не записываются.

## Ограничения

- Flight Bridge изменяет только уже существующие пользовательские профили MSFS
  2024. Если нужного профиля ещё нет, один раз сохраните его в игре и повторите
  запуск программы.
- Общие команды меню и камер могут отсутствовать в профильной категории самолёта
  MSFS 2024. Такие команды оставляются без изменений и показываются только в
  подробностях.
- Первая проверка результата внутри MSFS 2024 выполняется пользователем после
  установки этой сборки.
- Публичной Authenticode-подписи пока нет, поэтому Windows может показать обычное
  предупреждение неизвестного издателя.

© 2026 Denis Ugarov
