
using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Attribute = Application.Domain.Models.Attribute;

namespace Application.Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
              .HasOne(t => t.FromWallet)
              .WithMany(w => w.TransactionsFrom)
              .HasForeignKey(t => t.FromWalletId);

            modelBuilder.Entity<Transaction>()
              .HasOne(t => t.ToWallet)
              .WithMany(w => w.TransactionsTo)
              .HasForeignKey(t => t.ToWalletId);

            modelBuilder.Entity<User>()
              .HasIndex(p => p.EmailAddress).IsUnique();

            modelBuilder.Entity<Member>()
              .HasIndex(p => p.EmailAddress).IsUnique();

            modelBuilder.Entity<Project>()
                .HasIndex(p => p.ProjectName).IsUnique();

            modelBuilder.Entity<Project>()
                 .HasIndex(p => p.ProjectShortName).IsUnique();

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public DbSet<Role> Roles { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserFCMToken> UserFCMTokens { get; set; } = null!;

        public DbSet<Member> Member { get; set; } = null!;
        public DbSet<Notification> Notification { get; set; } = null!;
        public DbSet<MemberWallet> MemberWallets { get; set; } = null!;

        public DbSet<Level> Levels { get; set; } = null!;
        public DbSet<MemberLevel> MemberLevel { get; set; } = null!;

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectFile> ProjectFiles { get; set; } = null!;
        public DbSet<SystemFile> SystemFiles { get; set; } = null!;
        public DbSet<MemberFile> MemberFiles { get; set; } = null!;
        public DbSet<SponsorFile> SponsorFiles { get; set; } = null!;
        public DbSet<VoucherFile> VoucherFiles { get; set; } = null!;

        public DbSet<ProjectMilestone> ProjectMilestones { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<ProjectReportMemberTask> ProjectReportMemberTasks { get; set; } = null!;
        public DbSet<ProjectMemberRequest> ProjectMemberRequests { get; set; } = null!;
        public DbSet<ProjectEndRequest> ProjectEndRequests { get; set; } = null!;

        public DbSet<ProjectReport> ProjectReports { get; set; } = null!;
        public DbSet<ProjectReportMember> ProjectReportMembers { get; set; } = null!;

        public DbSet<ProjectWallet> ProjectWallets { get; set; } = null!;

        public DbSet<Sponsor> Sponsors { get; set; } = null!;
        public DbSet<ProjectSponsor> ProjectSponsors { get; set; } = null!;

        public DbSet<SalaryCycle> SalaryCycles { get; set; } = null!;

        public DbSet<ProjectMemberAttribute> ProjectMemberAttributes { get; set; } = null!;
        public DbSet<PayslipAttribute> PayslipAttributes { get; set; } = null!;
        public DbSet<PayslipItemAttribute> PayslipItemAttributes { get; set; } = null!;
        public DbSet<ProjectReportMemberAttribute> ProjectReportMemberAttributes { get; set; } = null!;
        public DbSet<Attribute> Attributes { get; set; } = null!;
        public DbSet<AttributeGroup> AttributeGroups { get; set; } = null!;

        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

        public DbSet<MemberVoucher> MemberVouchers { get; set; } = null!;
        public DbSet<Voucher> Vouchers { get; set; } = null!;

        // public DbSet<Ticket> Tickets { get; set; } = null!;
        // public DbSet<TicketDetail> TicketDetails { get; set; } = null!;

    }
}