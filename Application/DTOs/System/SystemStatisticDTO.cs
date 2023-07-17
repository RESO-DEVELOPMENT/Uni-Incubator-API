namespace Application.DTOs.System
{
    public class SystemStatisticDTO_Members
    {
        public int Total { get; set; }
        public int Admin { get; set; }
    }

    public class SystemStatisticDTO_Project
    {
        public int Total { get; set; }
        public int Created { get; set; }
        public int Started { get; set; }
        public int Ended { get; set; }
        public int Cancelled { get; set; }
    }

    public class SystemStatisticDTO_Sponsor
    {
        public int Total { get; set; }
    }


    public class SystemStatisticDTO
    {
        public SystemStatisticDTO_Members Members { get; set; } = new SystemStatisticDTO_Members();
        public SystemStatisticDTO_Project Projects { get; set; } = new SystemStatisticDTO_Project();
        public SystemStatisticDTO_Sponsor Sponsors { get; set; } = new SystemStatisticDTO_Sponsor();
    }
}