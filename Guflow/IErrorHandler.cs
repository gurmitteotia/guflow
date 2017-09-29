namespace Guflow
{
    internal interface IErrorHandler
    {
        ErrorAction OnError(Error error);
    }
}