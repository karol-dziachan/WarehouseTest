namespace KARacter.WarehouseTest.Application.Common.Interfaces.Services
{
    public interface IDateTime
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
