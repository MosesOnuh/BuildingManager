using System;
using System.Net;

namespace BuildingManager.Helpers
{
    public class RestException : Exception
    {
        public string ErrorMessage { get; set; }
        public HttpStatusCode Code { get; }
        public object Errors { get; }
        public override string Message { get { return !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage : base.Message; }}

        public RestException(HttpStatusCode code, string message, object errors = null)
        {
            ErrorMessage = message;
            Code = code;
            Errors = errors;
        }
    }
}
