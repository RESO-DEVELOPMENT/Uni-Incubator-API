using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class UnitOfWork
    {
        public readonly DataContext _dataContext;
        public UnitOfWork(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public AttributeRepository AttributeRepository => new(_dataContext);
        public AttributeGroupRepository AttributeGroupRepository => new(_dataContext);

        public UserRepository UserRepository => new(_dataContext);
        public UserFCMTokenRepository UserFCMTokenRepository => new(_dataContext);

        public MemberRepository MemberRepository => new(_dataContext);
        public NotificationRepository NotificationRepository => new(_dataContext);
        public MemberWalletRepository MemberWalletRepository => new(_dataContext);

        public ProjectRepository ProjectRepository => new(_dataContext);
        public ProjectMilestoneRepository ProjectMilestoneRepository => new(_dataContext);

        public SalaryCycleRepository SalaryCycleRepository => new(_dataContext);

        public ProjectMemberRepository ProjectMemberRepository => new(_dataContext);
        public ProjectReportRepository ProjectReportRepository => new(_dataContext);
        public ProjectReportMemberRepository ProjectReportMemberRepository => new(_dataContext);
        public ProjectEndRequestRepository ProjectEndRequestRepository => new(_dataContext);
        public ProjectReportMemberAttributeRepository ProjectReportMemberAttributeRepository => new(_dataContext);
        public ProjectReportMemberTaskRepository ProjectReportMemberTaskRepository => new(_dataContext);
        public ProjectWalletRepository ProjectWalletRepository => new(_dataContext);

        public ProjectReportMemberRepository ProjectMemberReportRepository => new(_dataContext);
        public ProjectMemberAttributeRepository ProjectMemberAttributeRepository => new(_dataContext);

        public ProjectMemberRequestRepository ProjectMemberRequestRepository => new(_dataContext);

        public WalletRepository WalletRepository => new(_dataContext);

        public TransactionRepository TransactionRepository => new(_dataContext);
        public RoleRepository RoleRepository => new(_dataContext);

        public SponsorRepository SponsorRepository => new(_dataContext);
        public ProjectSponsorRepository ProjectSponsorRepository => new(_dataContext);
        public ProjectSponsorTransactionRepository ProjectSponsorTransactionRepository => new(_dataContext);

        public PayslipRepository PayslipRepository => new(_dataContext);
        public PayslipItemRepository PayslipItemRepository => new(_dataContext);

        public LevelRepository LevelRepository => new(_dataContext);
        public SupplierRepository SupplierRepository => new(_dataContext);
        public MemberLevelRepository MemberLevelRepository => new(_dataContext);

        public VoucherRepository VoucherRepository => new(_dataContext);
        public MemberVoucherRepository MemberVoucherRepository => new(_dataContext);
        
        public async Task<bool> SaveAsync()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _dataContext.ChangeTracker.HasChanges();
        }

        public void ClearChanges()
        {
            _dataContext.ChangeTracker.Clear();
        }

        //public void RefreshContext()
        //{
        //    var context = _dataContext.Entry()
        //    var refreshableObjects = _dataContext.ChangeTracker.Entries().Select(c => c.Entity).ToList();
        //    context.Refresh(RefreshMode.StoreWins, refreshableObjects);

        //}

        public async Task EnsureDeleted()
        {
            await _dataContext.Database.EnsureDeletedAsync();
        }

        public async Task EnsureCreated()
        {
            await _dataContext.Database.EnsureCreatedAsync();
        }

    }
}