using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonApp.Data.DTOs
{
    public class CategoryAverageScoreResult
    {
        public string Category { get; set; } = null!;
        public double AverageScore { get; set; }
    }
}
