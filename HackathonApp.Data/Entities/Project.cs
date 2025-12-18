using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonApp.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }

        public string TeamName { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string Category { get; set; } = null!;

        public DateTime EventDate { get; set; }

        public decimal Score { get; set; }

        public int Members { get; set; }

        public string Captain { get; set; } = null!;
    }
}
