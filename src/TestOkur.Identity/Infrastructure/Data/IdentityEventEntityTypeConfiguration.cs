namespace TestOkur.Identity.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class IdentityEventEntityTypeConfiguration
        : IEntityTypeConfiguration<IdentityEvent>
    {
        public void Configure(EntityTypeBuilder<IdentityEvent> builder)
        {
            builder.ToTable("identity_events");
            builder.HasKey(_ => _.Id);
        }
    }
}
