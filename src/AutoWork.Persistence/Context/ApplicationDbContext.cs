using System.Linq.Expressions;
using AutoWork.Domain.Common;
using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<ChannelAccount> ChannelAccounts => Set<ChannelAccount>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostContent> PostContents => Set<PostContent>();
    public DbSet<PostSchedule> PostSchedules => Set<PostSchedule>();
    public DbSet<PostLog> PostLogs => Set<PostLog>();
    public DbSet<AiPrompt> AiPrompts => Set<AiPrompt>();
    public DbSet<AiGeneratedContent> AiGeneratedContents => Set<AiGeneratedContent>();
    public DbSet<FacebookAccount> FacebookAccounts => Set<FacebookAccount>();
    public DbSet<FacebookPage> FacebookPages => Set<FacebookPage>();
    public DbSet<WordPressSite> WordPressSites => Set<WordPressSite>();
    public DbSet<WordPressPost> WordPressPosts => Set<WordPressPost>();
    public DbSet<ZaloAccount> ZaloAccounts => Set<ZaloAccount>();
    public DbSet<ZaloPost> ZaloPosts => Set<ZaloPost>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Affiliate> Affiliates => Set<Affiliate>();
    public DbSet<AffiliateLink> AffiliateLinks => Set<AffiliateLink>();
    public DbSet<AffiliateCommission> AffiliateCommissions => Set<AffiliateCommission>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(entity => entity.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var param = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var prop = System.Linq.Expressions.Expression.Property(param, nameof(BaseEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(prop, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, param);
    }
}
