using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonApp.Data.Data
{
    public class HackathonContextFactory : IDesignTimeDbContextFactory<HackathonContext>
    {
        public HackathonContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HackathonContext>();

            optionsBuilder.UseSqlite("Data Source=Hackathon.db");

            return new HackathonContext(optionsBuilder.Options);
        }
    }
}

