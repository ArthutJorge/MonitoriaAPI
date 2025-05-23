using Microsoft.EntityFrameworkCore;
//using MonitoriaAPI.Models;

namespace MonitoriaAPI;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Monitor> Monitores => Set<Monitor>();
    public DbSet<Horario> Horarios => Set<Horario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        // relacionamento entre Horario e Monitor
        modelBuilder.Entity<Horario>()
            .HasOne<Monitor>()                // cada horário tem 1 monitor
            .WithMany(m => m.Horarios)        // cada monitor tem vários horários
            .HasForeignKey(h => h.IdMonitor);
        }
}
