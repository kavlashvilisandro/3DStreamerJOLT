namespace Streamer.Exceptions;

public class BaseResponseException : Exception
{
    public int StatusCode { get; set; }
    public BaseResponseException(int statusCode, string response) : base(response)
    {
        StatusCode = statusCode;
    }
}