namespace TestOkur.Identity.Services
{
    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure.Data;

    public class EventSink : IEventSink
    {
        private readonly ApplicationDbContextFactory _applicationDbContextFactory;

        public EventSink(ApplicationDbContextFactory applicationDbContextFactory)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }

        public Task PersistAsync(Event evt)
        {
            return Task.CompletedTask;
        }
    }
}
