using System.Xml.Linq;
using HackathonApp.Data.Data;
using HackathonApp.Data.DTOs;
using HackathonApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackathonApp.Data.Services
{
    // Delegate from assignment
    public delegate void DataImportedHandler(int inserted, int updated, int skipped, TimeSpan duration);

    public class HackathonService
    {
        private readonly HackathonContext _context;

        public event DataImportedHandler? DataImported;

        public HackathonService(HackathonContext context)
        {
            _context = context;
        }

        #region XML IMPORT

        public async Task ImportFromXmlAsync(string xmlPath)
        {
            var start = DateTime.UtcNow;

            int inserted = 0;
            int updated = 0;
            int skipped = 0;

            var messages = new List<string>();

            if (!File.Exists(xmlPath))
            {
                throw new FileNotFoundException($"XML file not found at path: {xmlPath}");
            }

            XDocument doc = XDocument.Load(xmlPath);

            var projectsXml = doc.Root?.Elements("Project") ?? Enumerable.Empty<XElement>();

            foreach (var p in projectsXml)
            {
                try
                {
                    var project = ParseProjectFromXml(p, out string? error);

                    if (project == null)
                    {
                        skipped++;
                        if (!string.IsNullOrWhiteSpace(error))
                            messages.Add(error);
                        continue;
                    }

                    // Upsert by Id
                    var existing = await _context.Projects
                        .FirstOrDefaultAsync(x => x.Id == project.Id);

                    if (existing == null)
                    {
                        await _context.Projects.AddAsync(project);
                        inserted++;
                    }
                    else
                    {
                        existing.TeamName = project.TeamName;
                        existing.ProjectName = project.ProjectName;
                        existing.Category = project.Category;
                        existing.EventDate = project.EventDate;
                        existing.Score = project.Score;
                        existing.Members = project.Members;
                        existing.Captain = project.Captain;
                        updated++;
                    }
                }
                catch (Exception ex)
                {
                    skipped++;
                    messages.Add($"Exception while processing record: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            var duration = DateTime.UtcNow - start;

            DataImported?.Invoke(inserted, updated, skipped, duration);

            if (messages.Any())
            {
                Console.WriteLine("Some records were skipped due to errors:");
                foreach (var msg in messages.Take(5))
                {
                    Console.WriteLine("  - " + msg);
                }
                if (messages.Count > 5)
                {
                    Console.WriteLine($"  ... and {messages.Count - 5} more.");
                }
            }
        }

        private Project? ParseProjectFromXml(XElement element, out string? error)
        {
            error = null;

            try
            {
                int id = (int)element.Element("Id")!;
                string teamName = (string)element.Element("TeamName")!;
                string projectName = (string)element.Element("ProjectName")!;
                string category = (string)element.Element("Category")!;
                string captain = (string)element.Element("Captain")!;
                string eventDateStr = (string)element.Element("EventDate")!;
                string scoreStr = (string)element.Element("Score")!;
                string membersStr = (string)element.Element("Members")!;

                if (id <= 0)
                {
                    error = $"Invalid Id: {id}";
                    return null;
                }

                if (string.IsNullOrWhiteSpace(teamName) ||
                    string.IsNullOrWhiteSpace(projectName) ||
                    string.IsNullOrWhiteSpace(category) ||
                    string.IsNullOrWhiteSpace(captain))
                {
                    error = $"Required string field missing for Id={id}.";
                    return null;
                }

                if (!DateTime.TryParse(eventDateStr, out var eventDate))
                {
                    error = $"Invalid EventDate for Id={id}.";
                    return null;
                }

                if (eventDate > DateTime.Today)
                {
                    error = $"EventDate is in the future for Id={id}.";
                    return null;
                }

                if (!decimal.TryParse(scoreStr,
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var score))
                {
                    error = $"Invalid Score for Id={id}.";
                    return null;
                }

                if (score < 0m || score > 100m)
                {
                    error = $"Score out of range (0–100) for Id={id}.";
                    return null;
                }

                if (!int.TryParse(membersStr, out var members))
                {
                    error = $"Invalid Members for Id={id}.";
                    return null;
                }

                if (members < 1 || members > 15)
                {
                    error = $"Members out of range (1–15) for Id={id}.";
                    return null;
                }

                return new Project
                {
                    Id = id,
                    TeamName = teamName.Trim(),
                    ProjectName = projectName.Trim(),
                    Category = category.Trim(),
                    Captain = captain.Trim(),
                    EventDate = eventDate,
                    Score = score,
                    Members = members
                };
            }
            catch (Exception ex)
            {
                error = $"General parse error: {ex.Message}";
                return null;
            }
        }

        #endregion

        #region SIMPLE QUERIES (1–5)

        // 1. All projects by team “NeuralNova” (query syntax)
        public async Task<List<Project>> GetProjectsByTeamNeuralNovaAsync()
        {
            var query =
                from p in _context.Projects
                where p.TeamName == "NeuralNova"
                select p;

            return await query.ToListAsync();
        }

        // 2. All projects submitted on 2025-10-12 (query syntax)
        public async Task<List<Project>> GetProjectsOn2025_10_12Async()
        {
            var targetDate = new DateTime(2025, 10, 12);

            var query =
                from p in _context.Projects
                where p.EventDate == targetDate
                select p;

            return await query.ToListAsync();
        }

        // 3. All projects in the AI-ML category (query syntax)
        public async Task<List<Project>> GetAiMlProjectsAsync()
        {
            var query =
                from p in _context.Projects
                where p.Category == "AI-ML"
                select p;

            return await query.ToListAsync();
        }

        // 4. Projects with Score > 90 (method syntax)
        public async Task<List<Project>> GetProjectsScoreAbove90Async()
        {
            var list = await _context.Projects
    .Where(p => p.Score > 90m)
    .ToListAsync();

            return list
                .OrderByDescending(p => p.Score)
                .ToList();

        }

        // 5. Top 5 highest-scoring projects overall (method syntax)
        public async Task<List<Project>> GetTop5ProjectsAsync()
        {
            var list = await _context.Projects.ToListAsync();

            return list
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.TeamName)
                .Take(5)
                .ToList();

        }

        #endregion

        #region MEDIUM_QUERIES

        // 6. Projects submitted between 2024-01-01 and 2024-12-31 (query syntax)
        public async Task<List<Project>> GetProjectsInYear2024Async()
        {
            var fromDate = new DateTime(2024, 1, 1);
            var to = new DateTime(2025, 1, 1);

            var query =
                from p in _context.Projects
                where p.EventDate >= fromDate && p.EventDate < to
                select p;

            return await query.ToListAsync();
        }

        // 7. HealthTech projects with Score > 88 (method syntax)
        public async Task<List<Project>> GetHealthTechProjectsScoreAbove88Async()
        {
            var list = await _context.Projects
      .Where(p => p.Category == "HealthTech" && p.Score > 88m)
      .ToListAsync();

            return list
                .OrderByDescending(p => p.Score)
                .ToList();

        }

        // 8. Projects sorted by EventDate asc, then Score desc (method syntax)
        public async Task<List<Project>> GetProjectsSortedByDateThenScoreAsync()
        {
            var list = await _context.Projects
                .ToListAsync();  // load all (filtering not needed)

            return list
                .OrderBy(p => p.EventDate)
                .ThenByDescending(p => p.Score)
                .ToList();
        }
        // 9. Count projects per category (query syntax)
        public async Task<List<CategoryCountResult>> GetProjectCountsPerCategoryAsync()
        {
            var query =
                from p in _context.Projects
                group p by p.Category
                into g
                orderby g.Key
                select new CategoryCountResult
                {
                    Category = g.Key,
                    Count = g.Count()
                };

            return await query.ToListAsync();
        }

        // 10. Top 3 projects by “ByteForge” (by score, method syntax)
        public async Task<List<Project>> GetTop3ByteForgeProjectsAsync()
        {
            var list = await _context.Projects
                .Where(p => p.TeamName == "ByteForge")
                .ToListAsync();

            return list
                .OrderByDescending(p => p.Score)
                .Take(3)
                .ToList();
        }


        #endregion

        #region COMPLEX QUERIES (11–15)

        // 11. Group by category and compute average score per category (query syntax)
        public async Task<List<CategoryAverageScoreResult>> GetCategoryAverageScoresAsync()
        {
            var query =
                from p in _context.Projects
                group p by p.Category
                into g
                orderby g.Key
                select new CategoryAverageScoreResult
                {
                    Category = g.Key,
                    AverageScore = g.Average(x => (double)x.Score)
                };

            return await query.ToListAsync();
        }

        // 12. SmartCity or Energy projects with Score >= category average (method syntax)
        public async Task<List<Project>> GetSmartCityOrEnergyAboveCategoryAverageAsync()
        {
            var averages = await GetCategoryAverageScoresAsync();

            var smartAvg = averages.FirstOrDefault(a => a.Category == "SmartCity")?.AverageScore ?? double.NaN;
            var energyAvg = averages.FirstOrDefault(a => a.Category == "Energy")?.AverageScore ?? double.NaN;

            var list = await _context.Projects
                .Where(p =>
                    (p.Category == "SmartCity" && !double.IsNaN(smartAvg) && (double)p.Score >= smartAvg) ||
                    (p.Category == "Energy" && !double.IsNaN(energyAvg) && (double)p.Score >= energyAvg))
                .ToListAsync();

            return list
                .OrderBy(p => p.Category)
                .ThenByDescending(p => p.Score)
                .ToList();
        }


        // 13. ProjectName contains “AI” and Score > 92 (method syntax)
        public async Task<List<Project>> GetProjectsWithAiInNameAndScoreAbove92Async()
        {
            var list = await _context.Projects
                .Where(p => p.Score > 92m)
                .ToListAsync();   // Bring to memory

            return list
                .Where(p => p.ProjectName.Contains("AI", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.Score)
                .ToList();
        }


        // 14. Top 5 by score within each category (method syntax)
        public async Task<List<Project>> GetTop5PerCategoryAsync()
        {
            var list = await _context.Projects.ToListAsync();

            return list
                .GroupBy(p => p.Category)
                .SelectMany(g => g
                    .OrderByDescending(p => p.Score)
                    .Take(5))
                .OrderBy(p => p.Category)
                .ThenByDescending(p => p.Score)
                .ToList();
        }


        // 15. Members ≥ 5 and Score above global average (query syntax)
        public async Task<List<Project>> GetBigTeamsAboveGlobalAverageAsync()
        {
            var globalAverage = await _context.Projects.AverageAsync(p => p.Score);

            var list = await _context.Projects
                .Where(p => p.Members >= 5 && p.Score > globalAverage)
                .ToListAsync();

            return list
                .OrderByDescending(p => p.Score)
                .ToList();
        }

        #endregion
    }
}
