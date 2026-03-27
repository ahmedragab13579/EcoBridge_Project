namespace EcoBridgeAPI.Result
{
    public class Result<T>
    {
        public T _value { get; set; }= default!;
        public bool _success { get; set; } = true;
        public string _message { get; set; } = string.Empty;



        public static Result.Result<T> Success(T value,string message)
        {
            return new Result.Result<T>
            {
                _success = true,
                _message = message,
                _value = value,

            };
        }


        public static Result.Result<T> Fail(T value,string message)
        {
            return new Result.Result<T>
            {
                _success = false,
                _message = message,
                _value = value,

            };
        }



    }
}
