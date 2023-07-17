namespace Application.Domain.Enums
{
    public static class TaskName
    {
        public static string SendPoint => "SEND_POINT";
        public static string SendXP => "SEND_XP";
        public static string SendMail => "SEND_MAIL";

        // public static string SetupWallet => "SETUP_WALLET";

        public static string MemberBuyVoucher => "MEMBER_BUY_VOUCHER";
        public static string CreateProjectSalaryCycle => "CREATE_PROJECT_SALARY_CYCLE";

        public static string UpdateProjectSalaryCycleStatus => "UPDATE_PROJECT_SALARY_CYCLE_STATUS";

        public static string ProcessSalaryCycleCreate => "PROCESS_SALARY_CYCLE_CHANGE_STATE_CREATE";
        public static string ProcessSalaryCyclePaid => "PROCESS_SALARY_CYCLE_PAID";
        public static string ProcessSalaryCycleCancel => "PROCESS_SALARY_CYCLE_CANCEL";

        public static string ProcessProjectEnd => "PROCESS_PROJECT_END";


        // public static string ProcessExpiredSalaryCycle => "PROCESS_EXPIRED_PROJECT_SALARY_CYCLE";

        public static string SendNotification => "SEND_NOTIIFICATION";

        public static string CheckExpiredWallets => "CHECK_EXPIRED_WALLETS";
        public static string CheckSalaryCycle => "CHECK_SALARY_CYCLES";
        // public static string CheckProjectSalaryCycles => "CHECK_PROJECT_SALARY_CYCLES";
        public static string CheckMembersLevel => "CHECK_MEMBERS_LEVEL";
        public static string CheckDisabledMemberWallet => "CHECK_DISABLED_MEMBER_WALLET";
        public static string CheckEndedProjectWallet => "CHECK_ENDED_PROJECT_WALLET";

        public static string CheckExpiredProject => "CHECK_EXPIRED_PROJECT";
        public static string CheckExpiredVoucher => "CHECK_EXPIRED_VOUCHERS";
    }
}