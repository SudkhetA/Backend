using Microsoft.EntityFrameworkCore;
using Backend.Models.System;

namespace Backend.Models
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        #region System
        public virtual DbSet<DocumentStatus> DocumentStatuses { get; set; }
        public virtual DbSet<User> Users { get; set;}
        public virtual DbSet<UserDocumentStatus> UserDocumentStatuses { get; set; }
        public virtual DbSet<UserNotification> UserNotifications { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set;}
        public virtual DbSet<Menu> Menus { get; set;}
        public virtual DbSet<MenuDocumentStatus> MenuDocumentStatuses { get; set; }
        public virtual DbSet<MenuType> MenuTypes { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Role> Roles { get; set;}
        public virtual DbSet<RoleMenu> RoleMenus { get; set;}
        public virtual DbSet<TransactionLog> TransactionLogs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(x => 
            {
                x.HasMany(e => e.DocumentStatuses)
                    .WithMany(e => e.Users)
                    .UsingEntity<UserDocumentStatus>(
                        l => l.HasOne(e => e.DocumentStatus).WithMany(e => e.UserDocumentStatuses),
                        r => r.HasOne(e => e.User).WithMany(e => e.UserDocumentStatuses));

                x.HasMany(e => e.Notifications)
                    .WithMany(e => e.Users)
                    .UsingEntity<UserNotification>(
                        l => l.HasOne(e => e.Notification).WithMany(e => e.UserNotifications),
                        r => r.HasOne(e => e.User).WithMany(e => e.UserNotifications));
                
                x.HasMany(e => e.Roles)
                    .WithMany(e => e.Users)
                    .UsingEntity<UserRole>(
                        l => l.HasOne(e => e.Role).WithMany(e => e.UserRoles),
                        r => r.HasOne(e => e.User).WithMany(e => e.UserRoles));
            });

            modelBuilder.Entity<Menu>(x =>
            {
                x.HasMany(e => e.DocumentStatuses)
                    .WithMany(e => e.Menus)
                    .UsingEntity<MenuDocumentStatus>(
                        l => l.HasOne(e => e.DocumentStatus).WithMany(e => e.MenuDocumentStatuses),
                        r => r.HasOne(e => e.Menu).WithMany(e => e.MenuDocumentStatuses));
            });

            modelBuilder.Entity<Role>(x =>
            {
                x.HasMany(e => e.Menus)
                    .WithMany(e => e.Roles)
                    .UsingEntity<RoleMenu>(
                        l => l.HasOne(e => e.Menu).WithMany(e => e.RoleMenus),
                        r => r.HasOne(e => e.Role).WithMany(e => e.RoleMenus));
            });

            modelBuilder.Entity<MenuType>(x => 
            {
                x.Property(e => e.Name)
                    .HasConversion(
                        v => v.ToString(),
                        v => (EnumMenuType)Enum.Parse(typeof(EnumMenuType), v!));
            });

            modelBuilder.Entity<TransactionLog>(x => 
            {
                x.Property(e => e.OperationType)
                    .HasConversion(
                        v => v.ToString(),
                        v => (EnumOperationType)Enum.Parse(typeof(EnumOperationType), v));
            });
        }

    }
}
