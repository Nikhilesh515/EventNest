using EventNest.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventNest.AuthService.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .HasColumnName("id");

        builder.Property(rp => rp.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rp => rp.PermissionName)
            .HasColumnName("permission_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rp => rp.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionName })
            .IsUnique()
            .HasDatabaseName("idx_role_permissions_role_permission");

        builder.HasIndex(rp => rp.RoleId)
            .HasDatabaseName("idx_role_permissions_role_id");

        builder.HasOne(rp => rp.Role)
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
