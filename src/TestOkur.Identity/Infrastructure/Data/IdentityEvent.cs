namespace TestOkur.Identity.Infrastructure.Data
{
    using System;

    public class IdentityEvent
    {
        public IdentityEvent()
        {
            Id = Guid.NewGuid();
            TimestampUtc = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public DateTime TimestampUtc { get; set; }

        public string EventType { get; set; }

        public string EventJson { get; set; }
    }
}
