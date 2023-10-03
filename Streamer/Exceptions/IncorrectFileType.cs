namespace Streamer.Exceptions;

public class IncorrectFileType : BaseResponseException
{
    public IncorrectFileType() : base(400, "Incorrect file type")
    {
        
    }
}