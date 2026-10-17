namespace Syner.Api.Configuration;

public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    // Ruta absoluta guardada en User Secrets o en las variables del hosting.
    public string? ServiceAccountPath { get; init; }
}
