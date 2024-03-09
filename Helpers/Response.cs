namespace BuildingManager.Helpers
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "Success";
    }

    public class SuccessResponse<T> : Response
    {
        public SuccessResponse()
        {
            Success = true;
        }

        public T? Data { get; set; }
    }

    public class ErrorResponse<T> : Response
    {
        public ErrorResponse()
        {
            base.Success = false;
        }
        public T? Error { get; set; }
    }
}
