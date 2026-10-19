using Microsoft.EntityFrameworkCore;
using UX.ControlPlane.Data.Entities;

namespace UX.ControlPlane.Data;

public class UxDbContext : DbContext
{
    public UxDbContext(DbContextOptions<UxDbContext> options) : base(options) { }

    public DbSet<OrgNode> OrgNodes => Set<OrgNode>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentTelemetryLatest> AgentTelemetryLatest => Set<AgentTelemetryLatest>();
    public DbSet<ConfigProfile> ConfigProfiles => Set<ConfigProfile>();
    public DbSet<ConfigAssignment> ConfigAssignments => Set<ConfigAssignment>();
    public DbSet<ConfigVersion> ConfigVersions => Set<ConfigVersion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrgNode>().HasIndex(e => e.ParentId);

        modelBuilder.Entity<Agent>().HasIndex(e => e.OrgNodeId);
        modelBuilder.Entity<Agent>().HasIndex(e => e.LastSeenUtc);
        modelBuilder.Entity<Agent>().HasIndex(e => e.MachineName);

        modelBuilder.Entity<Agent>()
            .HasOne(a => a.LatestTelemetry)
            .WithOne(t => t.Agent)
            .HasForeignKey<AgentTelemetryLatest>(t => t.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConfigAssignment>()
            .HasIndex(e => new { e.ScopeType, e.ScopeId });

        modelBuilder.Entity<ConfigVersion>()
            .HasIndex(e => new { e.ScopeType, e.ScopeId, e.Hash });

        modelBuilder.Entity<AuditLog>().HasIndex(e => e.Utc);
        modelBuilder.Entity<AuditLog>().HasIndex(e => e.Entity);
        modelBuilder.Entity<AuditLog>().HasIndex(e => e.Actor);
    }
}
