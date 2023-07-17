using Application.Domain.Enums.Wallet;

namespace Application.Helpers
{
    public static class TransactionHelper
    {
        /// <summary>
        /// Get tuples
        /// </summary>
        public static (TargetType, TargetType)? GetTargetTypesFromTransactionType(TransactionType type)
        {
            switch (type)
            {
                case TransactionType.MemberToMember:
                    {
                        return (TargetType.Member, TargetType.Member);
                    }
                case TransactionType.ProjectSalary:
                    {
                        return (TargetType.Project, TargetType.Member);
                    }
                case TransactionType.SystemDepositToProject:
                case TransactionType.SponsorDepositToProject:
                    {
                        return (TargetType.System, TargetType.Project);
                    }
                case TransactionType.ProjectBonus:
                    {
                        return (TargetType.Project, TargetType.Member);
                    }
                case TransactionType.SystemSalary:
                    {
                        return (TargetType.System, TargetType.Member);
                    }
                case TransactionType.BuyVoucher:
                    {
                        return (TargetType.Member, TargetType.System);
                    }
                case TransactionType.ProjectToProject:
                    {
                        return (TargetType.Project, TargetType.Project);
                    }
                case TransactionType.ProjectReturnToSystem:
                    {
                        return (TargetType.Project, TargetType.System);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}