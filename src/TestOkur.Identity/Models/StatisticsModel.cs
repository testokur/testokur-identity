namespace TestOkur.Identity.Models
{
    public class StatisticsModel
    {
        public string ExpiredUsersToday { get; set; }

        public int TotalIndividualLoginCountInDay { get; set; }

        public int TotalUserCount { get; set; }

        public int TotalActiveUserCount { get; set; }

        public int NewUserActivatedCountToday { get; set; }

        public int SubscriptionExtendedCountToday { get; set; }
    }
}
