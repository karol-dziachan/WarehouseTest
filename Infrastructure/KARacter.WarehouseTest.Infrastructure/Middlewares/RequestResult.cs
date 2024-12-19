using KARacter.WarehouseTest.Application.Common.Abstractions;

namespace KARacter.WarehouseTest.Infrastructure.Middlewares
{
    public class RequestResult : BaseResult
    {
        public RequestResult(bool success, string message) : base(success, message) { }
    }
}
