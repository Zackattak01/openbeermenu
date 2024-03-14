namespace OpenBeerMenu.Sync
{
    public class SyncRequestResult
    {
        public static SyncRequestResult Success = new SyncRequestResult(true, "Successful");

        public bool IsSuccess { get; }
        public string Message { get; }

        public SyncRequestResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }
    }

    public class SyncRequestResult<T> : SyncRequestResult
    {
        public T Value { get; set; }

        public SyncRequestResult(T value) : base(true, "Successful")
        {
            Value = value; 
        }

        public SyncRequestResult(bool isSuccess, string message) : base(isSuccess, message)
        { }
    }
}
