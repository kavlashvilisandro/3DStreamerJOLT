namespace Streamer.Exceptions;

public class FileAlreadyExists : BaseResponseException
{
    public FileAlreadyExists() : base(400, "File already exists")
    {
        
    }
}