namespace Syner.Api.Services.Archivos;

public sealed class AlmacenamientoImagenesException : Exception
{
    public AlmacenamientoImagenesException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
