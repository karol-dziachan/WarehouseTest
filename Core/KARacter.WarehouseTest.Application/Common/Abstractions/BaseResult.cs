namespace KARacter.WarehouseTest.Application.Common.Abstractions
{
    public abstract class BaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } 

        public BaseResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
