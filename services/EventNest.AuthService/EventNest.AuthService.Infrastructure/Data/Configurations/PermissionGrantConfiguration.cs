using EventNest.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventNest.AuthService.Infrastructure.Data.Configurations;

public class PermissionGrantConfiguration : IEntityTypeConfiguration<PermissionGrant>
{
    public void Configure(EntityTypeBuilder<PermissionGrant> builder)
    {
        builder.ToTable("permission_grants");

        builder.HasKey(pg => pg.Id);

        builder.Property(pg => pg.Id)
            .HasColumnName("id");

        builder.Property(pg => pg.PermissionName)
            .HasColumnName("permission_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(pg => pg.IsGranted)
            .HasColumnName("is_granted")
            .HasDefaultValue(true);

        builder.Property(pg => pg.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(pg => pg.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(pg => pg.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(pg => pg.UserId)
            .HasDatabaseName("idx_permission_grants_user_id");

        builder.HasIndex(pg => new { pg.UserId, pg.PermissionName })
            .HasDatabaseName("idx_permission_grants_user_permission");

        builder.HasOne(pg => pg.User)
            .WithMany(u => u.PermissionGrants)
            .HasForeignKey(pg => pg.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
