# Legacy Parser

C#-часть системы расписания. Это основной парсер Excel-файлов, написанный для .NET Framework 4.5.2.

## Основные файлы

- `ConsoleParser/Program.cs` - запуск, выбор reader'а, обработка папок, загрузка словаря.
- `ConsoleParser/reader1.cs` - чтение очного расписания.
- `ConsoleParser/reader2.cs` - чтение заочного расписания.
- `ConsoleParser/reader3.cs` - чтение экзаменов.
- `ConsoleParser/Yacheyka.cs` - разбор текста внутри одной ячейки расписания.
- `ConsoleParser/WriterDB.cs` - запись результата в MySQL.
- `Tester/UnitTest1.cs` - тестовые примеры для парсинга ячеек, дат и времени.

## Важное

Это исторический код. Он сохранен почти в исходном стиле, но конфигурационные значения в `App.config` и `allsetting.settings` заменены на безопасные placeholders.

Старые `packages/`, `bin/` и `obj/` не включены. Для реального запуска понадобилось бы восстановление NuGet-пакетов и схема MySQL-БД.

