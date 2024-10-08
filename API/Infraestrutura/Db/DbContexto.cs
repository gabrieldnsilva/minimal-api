using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entities;

namespace minimal_api.Infraestrutura.DB;

    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
    public DbSet<Administrators> Administrators { get; set; } = default!;
    public DbSet<Vehicles> Vehicles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrators>().HasData(
            new Administrators {
                Id = 1,
                Email = "admin@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            }
        );
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConnection = _configuracaoAppSettings.GetConnectionString("MySQLConnection")?.ToString();

            if (!string.IsNullOrEmpty(stringConnection))
            {
                optionsBuilder.UseMySql(stringConnection, 
                ServerVersion.AutoDetect(stringConnection));
            }             
        }
    }
} 

