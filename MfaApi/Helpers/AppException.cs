using System.Globalization;
using System.Net;

namespace MfaApi.Helpers;

// custom exception class for throwing application specific exceptions 
// that can be caught and handled within the application
public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.BadRequest;

    public AppException() : base() { }

    public AppException(string message) : base(message) { }

    public AppException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public AppException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}