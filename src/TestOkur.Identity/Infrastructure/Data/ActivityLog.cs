namespace TestOkur.Identity.Infrastructure.Data
{
    using System;

    public class ActivityLog
    {
        public ActivityLog()
        {
        }

        public ActivityLog(ActivityLogType type, string userId, string createdBy)
        {
            DateTimeUtc = DateTime.UtcNow;
            Type = type;
            UserId = userId;
            CreatedBy = createdBy;
        }

        public long Id { get; set; }

        public string UserId { get; set; }

        public ActivityLogType Type { get; set; }

        public DateTime DateTimeUtc { get; set; }

        public string CreatedBy { get; set; }
    }
}
