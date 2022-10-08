using System.Globalization;

Console.WriteLine("Укажите путь до папки с фото(По умолчанию - данная директория)");
var path = Console.ReadLine() ?? ".";
path = path.Trim() == "" ? "." : path;
Dictionary<(string, string), string> filesMap = new Dictionary<(string, string), string>();
Console.WriteLine("Дубликаты: ");
foreach (var filePath in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
{
    var fileInfo = new FileInfo(filePath);
    if (filesMap.ContainsKey((fileInfo.Name, fileInfo.CreationTime.ToString(DateTimeFormatInfo.CurrentInfo))))
        Console.WriteLine(
            $"{fileInfo.FullName} {filesMap[(fileInfo.Name, fileInfo.CreationTime.ToString(CultureInfo.CurrentCulture))]}");
    else
        filesMap.Add((fileInfo.Name, fileInfo.CreationTime.ToString(CultureInfo.CurrentCulture)),
            fileInfo.FullName);
}

idiot:
Console.WriteLine("Выберите период (d - day, w - week, m - month)");
var period = Console.ReadKey();
Dictionary<string, List<string>> filesSorted = new Dictionary<string, List<string>>();

foreach (var pair in filesMap.Keys)
{
    var createdTime = DateTime.Parse(pair.Item2);
    try
    {
        string key = period.Key switch
        {
            ConsoleKey.D => $"{createdTime.DayOfYear} {createdTime.Year}",
            ConsoleKey.W => $"{createdTime.DayOfYear / 7} {createdTime.Year}",
            ConsoleKey.M => $"{createdTime.Month} {createdTime.Year}",
            _ => throw new ArgumentOutOfRangeException()
        };
        if (filesSorted.ContainsKey(key))
        {
            filesSorted[key].Add(filesMap[pair]);
        }
        else
            filesSorted[key] = new List<string>() {filesMap[pair]};
    }
    catch (ArgumentOutOfRangeException e)
    {
        goto idiot;
    }
}

Directory.SetCurrentDirectory(path);
Directory.CreateDirectory("sorted");
Directory.SetCurrentDirectory("sorted");
foreach (var dateString in filesSorted.Keys)
{
    var ints = dateString.Split().Select(s => int.Parse(s)).ToArray();
    (int what, int year) = (ints[0], ints[1]);
    string folderName = period.Key switch
    {
        ConsoleKey.D => $"{new DateTime(year, 1, 1).AddDays(what - 1).ToShortDateString().Replace("/", "-")}",
        ConsoleKey.W => new DateTime(year, 1, 1).AddDays(what * 7).ToShortDateString().Replace("/", "-") + " - " +
                        new DateTime(year, 1, 1).AddDays(what * 7 + 7).ToShortDateString().Replace("/", "-"),
        ConsoleKey.M =>
            $"{new DateTime(year, what, 1).ToShortDateString().Replace("/", "-")} - {new DateTime(year, what, 1).AddMonths(1).AddDays(-1).ToShortDateString().Replace("/", "-")}",
    };
    Console.WriteLine(folderName);
    Directory.CreateDirectory(folderName);
    foreach (var filePath in filesSorted[dateString])
    {
        int fileRepeat = 0;
        again:
        try
        {
            var newFileName = new FileInfo(filePath).Name.Split('.')[0] + (fileRepeat == 0 ? "" : fileRepeat.ToString()) + "." +
                              new FileInfo(filePath).Name.Split('.')[1];
            File.Copy(filePath, folderName + "\\" + newFileName);
        }
        catch (System.IO.IOException e)
        {
            fileRepeat++;
            goto again;
        }
    }
}