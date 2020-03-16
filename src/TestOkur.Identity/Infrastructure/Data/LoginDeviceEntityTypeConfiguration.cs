namespace TestOkur.Identity.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class LoginDeviceEntityTypeConfiguration
        : IEntityTypeConfiguration<LoginDevice>
    {
        public void Configure(EntityTypeBuilder<LoginDevice> builder)
        {
            builder.ToTable("login_devices");
            builder.HasKey(_ => _.Id);
            builder.HasOne<ApplicationUser>()
                .WithMany(_ => _.LoginDevices)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
