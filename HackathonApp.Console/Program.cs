using HackathonApp.Data.Data;
using HackathonApp.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HackathonApp.ConsoleApp;

class Program
{
    private static Dictionary<string, object> lastQueryResults = new();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Load appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string baseDir = Directory.GetCurrentDirectory();

        string xmlPath = Path.Combine(baseDir, config["Paths:XmlInput"]!);
        string jsonOutputDir = Path.Combine(baseDir, config["Paths:JsonOutput"]!);


        // Setup EF Core context
        var options = new DbContextOptionsBuilder<HackathonContext>()
            .UseSqlite(config.GetConnectionString("HackathonDb"))
            .Options;

        using var context = new HackathonContext(options);
        await context.Database.EnsureCreatedAsync();

        var service = new HackathonService(context);

        // Event: Show import summary
        service.DataImported += (inserted, updated, skipped, duration) =>
        {
            Console.WriteLine("\n=== Import completed ===");
            Console.WriteLine($"Inserted: {inserted}");
            Console.WriteLine($"Updated : {updated}");
            Console.WriteLine($"Skipped : {skipped}");
            Console.WriteLine($"Duration: {duration.TotalMilliseconds} ms");
            Console.WriteLine("========================\n");
        };

        // ---------------- MENU LOOP ----------------
        while (true)
        {
            Console.WriteLine("===== Hackathon Results Management System =====");
            Console.WriteLine("1) Import XML -> Database");
            Console.WriteLine("2) Run Simple LINQ queries");
            Console.WriteLine("3) Run Medium LINQ queries");
            Console.WriteLine("4) Run Complex LINQ queries");
            Console.WriteLine("5) Export last query results to JSON (manual)");
            Console.WriteLine("0) Exit");
            Console.Write("Choose an option: ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await service.ImportFromXmlAsync(xmlPath);
                    break;

                case "2":
                    await RunSimpleQueriesAsync(service, jsonOutputDir);
                    break;

                case "3":
                    await RunMediumQueriesAsync(service, jsonOutputDir);
                    break;

                case "4":
                    await RunComplexQueriesAsync(service, jsonOutputDir);
                    break;

                case "5":
                    await ManualExportAll(jsonOutputDir);
                    break;

                case "0":
                    Console.WriteLine("\nExiting...");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Try again.\n");
                    break;
            }
        }
    }

    // ---------------- SIMPLE QUERIES ----------------
    private static async Task RunSimpleQueriesAsync(HackathonService service, string outputDir)
    {
        Console.WriteLine("\n>>> SIMPLE QUERIES (1-5)\n");

        // Q1
        var q1 = await service.GetProjectsByTeamNeuralNovaAsync();
        PrintProjects("Q1 - Projects by team 'NeuralNova'", q1);
        await AutoExport("q01_team_neuralnova.json", q1, outputDir);

        // Q2
        var q2 = await service.GetProjectsOn2025_10_12Async();
        PrintProjects("Q2 - Projects on 2025-10-12", q2);
        await AutoExport("q02_projects_on_date.json", q2, outputDir);

        // Q3
        var q3 = await service.GetAiMlProjectsAsync();
        PrintProjects("Q3 - Projects in AI-ML category", q3);
        await AutoExport("q03_aiml_projects.json", q3, outputDir);

        // Q4
        var q4 = await service.GetProjectsScoreAbove90Async();
        PrintProjects("Q4 - Projects with Score > 90", q4);
        await AutoExport("q04_score_above_90.json", q4, outputDir);

        // Q5
        var q5 = await service.GetTop5ProjectsAsync();
        PrintProjects("Q5 - Top 5 highest-scoring projects", q5);
        await AutoExport("q05_top5_projects.json", q5, outputDir);
    }

    // ---------------- MEDIUM QUERIES ----------------
    private static async Task RunMediumQueriesAsync(HackathonService service, string outputDir)
    {
        Console.WriteLine("\n>>> MEDIUM QUERIES (6-10)\n");

        var q6 = await service.GetProjectsInYear2024Async();
        PrintProjects("Q6 - Projects in the year 2024", q6);
        await AutoExport("q06_projects_2024.json", q6, outputDir);

        var q7 = await service.GetHealthTechProjectsScoreAbove88Async();
        PrintProjects("Q7 - HealthTech projects with Score > 88", q7);
        await AutoExport("q07_healthtech_above88.json", q7, outputDir);

        var q8 = await service.GetProjectsSortedByDateThenScoreAsync();
        PrintProjects("Q8 - Projects sorted by date asc, score desc", q8);
        await AutoExport("q08_sorted_by_date_score.json", q8, outputDir);

        var q9 = await service.GetProjectCountsPerCategoryAsync();
        PrintCategoryCounts(q9);
        await AutoExport("q09_category_counts.json", q9, outputDir);

        var q10 = await service.GetTop3ByteForgeProjectsAsync();
        PrintProjects("Q10 - Top 3 ByteForge projects", q10);
        await AutoExport("q10_top3_byteforge.json", q10, outputDir);
    }

    // ---------------- COMPLEX QUERIES ----------------
    private static async Task RunComplexQueriesAsync(HackathonService service, string outputDir)
    {
        Console.WriteLine("\n>>> COMPLEX QUERIES (11-15)\n");

        var q11 = await service.GetCategoryAverageScoresAsync();
        PrintAverageScores(q11);
        await AutoExport("q11_average_scores.json", q11, outputDir);

        var q12 = await service.GetSmartCityOrEnergyAboveCategoryAverageAsync();
        PrintProjects("Q12 - SmartCity/Energy above category average", q12);
        await AutoExport("q12_smartcity_energy_above_avg.json", q12, outputDir);

        var q13 = await service.GetProjectsWithAiInNameAndScoreAbove92Async();
        PrintProjects("Q13 - Projects with 'AI' in name & Score > 92", q13);
        await AutoExport("q13_ai_name_above92.json", q13, outputDir);

        var q14 = await service.GetTop5PerCategoryAsync();
        PrintProjects("Q14 - Top 5 per category", q14);
        await AutoExport("q14_top5_per_category.json", q14, outputDir);

        var q15 = await service.GetBigTeamsAboveGlobalAverageAsync();
        PrintProjects("Q15 - Big teams above global average", q15);
        await AutoExport("q15_big_teams_above_avg.json", q15, outputDir);
    }

    // ---------------- HELPERS ----------------
    private static async Task AutoExport(string fileName, object data, string outputDir)
    {
        await JsonExporter.ExportAsync(outputDir, fileName, data);
        Console.WriteLine($"   JSON exported: {fileName}\n");
    }

    private static async Task ManualExportAll(string outputDir)
    {
        foreach (var pair in lastQueryResults)
        {
            await JsonExporter.ExportAsync(outputDir, pair.Key + ".json", pair.Value);
        }

        Console.WriteLine($"Exported  JSON file(s) to: {outputDir}\n");
    }

    private static void PrintProjects(string header, IEnumerable<dynamic> list)
    {
        Console.WriteLine(header);
        Console.WriteLine(new string('-', 90));
        Console.WriteLine("Id  Team         Project         Category   EventDate   Score Captain");
        Console.WriteLine(new string('-', 90));

        foreach (var p in list)
        {
            Console.WriteLine($"{p.Id,-3} {p.TeamName,-12} {p.ProjectName,-14} {p.Category,-10} {p.EventDate:yyyy-MM-dd} {p.Score,6} {p.Captain}");
        }

        Console.WriteLine(new string('-', 90) + "\n");
    }

    private static void PrintCategoryCounts(IEnumerable<dynamic> list)
    {
        Console.WriteLine("Category        Count");
        Console.WriteLine("-----------------------------");
        foreach (var x in list)
            Console.WriteLine($"{x.Category,-15} {x.Count}");
        Console.WriteLine("-----------------------------\n");
    }

    private static void PrintAverageScores(IEnumerable<dynamic> list)
    {
        Console.WriteLine("Category        Avg Score");
        Console.WriteLine("-----------------------------");
        foreach (var x in list)
            Console.WriteLine($"{x.Category,-15} {x.AverageScore:F2}");
        Console.WriteLine("-----------------------------\n");
    }
}
