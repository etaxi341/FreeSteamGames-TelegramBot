using System;
using System.Collections.Generic;
using System.Text;
using DataManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Data
{
    public class DatabaseContext : DbContext
    {
        static bool wasMigrated = false;
        public DatabaseContext()
        {
            if (!wasMigrated)
            {
                wasMigrated = true;
                Database.Migrate();
            }
        }

        public DbSet<Subscribers> subscribers { get; set; }
        public DbSet<Notifications> notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Database.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
