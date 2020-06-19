namespace TestOkur.Identity.Services
{
    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class EventSink : IEventSink
    {
        private readonly ILogger<EventSink> _logger;

        public EventSink(ILogger<EventSink> logger)
        {
            _logger = logger;
        }

        public Task PersistAsync(Event evt)
        {
            _logger.LogInformation(evt?.ToString());
            return Task.CompletedTask;
        }
    }
}
