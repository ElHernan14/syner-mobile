namespace Syner.Api.Services.Archivos;

public sealed class SupabaseStorageOptions
{
    public const string SectionName = "SupabaseStorage";

    public string? Url { get; set; }
    public string? Bucket { get; set; }
    public string? ServiceRoleKey { get; set; }

    public bool IsValid()
    {
        var anyConfigured = new[] { Url, Bucket, ServiceRoleKey }
            .Any(value => !string.IsNullOrWhiteSpace(value));

        if (!anyConfigured)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(Url) &&
            Uri.TryCreate(Url, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            !string.IsNullOrWhiteSpace(Bucket) &&
            !string.IsNullOrWhiteSpace(ServiceRoleKey);
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Url) &&
        !string.IsNullOrWhiteSpace(Bucket) &&
        !string.IsNullOrWhiteSpace(ServiceRoleKey);
}
