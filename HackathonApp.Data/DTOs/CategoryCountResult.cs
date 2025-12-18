using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonApp.Data.DTOs
{

    public class CategoryCountResult
    {
        public string Category { get; set; } = null!;
        public int Count { get; set; }
    }
}
