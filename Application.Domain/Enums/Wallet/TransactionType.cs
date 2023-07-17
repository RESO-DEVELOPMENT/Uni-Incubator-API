namespace Application.Domain.Enums.Wallet
{
    public enum TransactionType
    {
        ProjectSalary,
        SystemSalary,
        WalletExpire,

        SystemDepositToProject,
        SponsorDepositToProject,

        ProjectBonus,
        ProjectToProject,
        ProjectReturnToSystem,

        BuyVoucher,

        MemberToMember = 100,

        MemberDisableReturnToSystem = 200,


        NewAccount = 999 // Testing
    }
}