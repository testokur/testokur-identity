namespace TestOkur.Identity.Infrastructure.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;
    using System.Threading.Tasks;
    using TestOkur.Infrastructure.Data;
    using TestOkur.Infrastructure.Mvc.Extensions;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, ICanMigrate
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IdentityEvent> Events { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public void Migrate()
        {
            Database.Migrate();
        }

        public async Task MigrateAsync()
        {
            await Database.MigrateAsync();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
            builder.ToSnakeCase();
        }
    }
}
