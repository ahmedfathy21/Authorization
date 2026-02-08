using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft;
using AuthSystemAPI.Models;
using AuthSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthSystemAPI.Data
{
    // Crucial: Inherit from IdentityDbContext, NOT just DbContext
    // This automatically adds tables for Users, Roles, Claims, and Tokens.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Custom DbSets for Permission-Based Authorization
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LoginUser> LoginUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // Disable external login provider storage (UserLogins table)
            builder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>();

            // Configure Permission entity
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(500);
            });

            // Configure RolePermission entity (Many-to-Many junction table)
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => rp.Id);

                // Foreign key to Role
                entity.HasOne(rp => rp.Role)
                    .WithMany()
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to Permission
                entity.HasOne(rp => rp.Permission)
                    .WithMany()
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ensure unique combination of RoleId and PermissionId
                entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
            });

            // Configure RefreshToken entity
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(200);
                entity.HasIndex(rt => rt.Token).IsUnique();

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LoginUser entity
            builder.Entity<LoginUser>(entity=>
            {
            entity.HasKey(lu => lu.Id);
            entity.Property(lu => lu.UserId).IsRequired();
            entity.Property(lu => lu.IpAddress).HasMaxLength(64);
            entity.Property(lu => lu.UserAgent).HasMaxLength(256);

            entity.HasOne(lu => lu.User)
                .WithMany(u => u.LoginUsers)
                .HasForeignKey(lu => lu.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
            });
        }
    }
    
}