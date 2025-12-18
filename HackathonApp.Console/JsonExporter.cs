using System.Text.Json;

namespace HackathonApp.ConsoleApp;

public static class JsonExporter
{
    public static async Task ExportAsync(string folder, string fileName, object data)
    {
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, fileName);

        var json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions { WriteIndented = true }
        );

        await File.WriteAllTextAsync(path, json);
    }
}
