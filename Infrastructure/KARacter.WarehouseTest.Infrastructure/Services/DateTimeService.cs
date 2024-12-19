using KARacter.WarehouseTest.Application.Common.Interfaces.Services;

namespace KARacter.WarehouseTest.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now =>
            DateTime.Now;

        public DateTime UtcNow =>
            DateTime.UtcNow;
    }
}
