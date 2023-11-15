using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace IziHardGames.DataBases.Abstreactions.Lib.PostgreSQL
{
    public class ExamplePostgreSqlDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=NutakuHeroes;Username=postgres;Password=postgres");
        }
    }
}