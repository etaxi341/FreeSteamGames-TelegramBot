using System.Threading;
using DataManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Data;

public class DatabaseContext : DbContext
{
    static bool wasMigrated = false;
    static bool isMigrating = false;
    public DatabaseContext()
    {
        while (isMigrating)
        {
            Thread.Sleep(1000);
        }
        if (!wasMigrated)
        {
            isMigrating = true;
            Database.Migrate();
            wasMigrated = true;
            isMigrating = false;
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