namespace Guflow
{
    public interface IErrorHandler
    {
        ErrorAction OnError(Error error);
    }
}