
using TenantDesk.Application.Interfaces.Services;

namespace TenantDesk.Infrastructure.Services;
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Today => DateTime.UtcNow.Date;
    
    }
