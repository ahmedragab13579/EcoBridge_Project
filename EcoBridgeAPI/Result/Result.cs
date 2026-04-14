namespace EcoBridgeAPI.Result
{
    public class Result<T>
    {
        public T Value { get; set; } = default!;
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;

        public static Result<T> SuccessResult(T value, string message)
        {
            return new Result<T>
            {
                Success = true,
                Message = message,
                Value = value,
            };
        }

        public static Result<T> FailResult(T value, string message)
        {
            return new Result<T>
            {
                Success = false,
                Message = message,
                Value = value,
            };
        }
    }
}
