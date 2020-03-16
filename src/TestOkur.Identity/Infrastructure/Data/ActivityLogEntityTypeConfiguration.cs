namespace TestOkur.Identity.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class ActivityLogEntityTypeConfiguration
        : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("activity_logs");
            builder.HasKey(_ => _.Id);
            builder.HasIndex(_ => _.UserId);
            builder.HasIndex(_ => new { _.DateTimeUtc, _.Type });
        }
    }
}
