namespace InnovatEPAM.Portal.Utilities;

/// <summary>
/// Provides secure file storage paths and MIME-type detection via magic bytes.
/// </summary>
public static class FileStorageHelper
{
    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { "application/pdf",  new byte[] { 0x25, 0x50, 0x44, 0x46 } },
        { "image/jpeg",       new byte[] { 0xFF, 0xD8, 0xFF } },
        { "image/png",        new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { "application/msword", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        { "application/zip",  new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
    };

    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/msword",
        "application/zip",
    };

    public static async Task<string?> DetectMimeTypeAsync(IFormFile file)
    {
        var header = new byte[16];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(0, Math.Min(16, (int)file.Length)));

        foreach (var (mime, magic) in MagicBytes)
        {
            if (read >= magic.Length && header.Take(magic.Length).SequenceEqual(magic))
                return mime;
        }
        return null;
    }

    public static bool IsAllowedMimeType(string? mimeType) =>
        mimeType != null && AllowedMimeTypes.Contains(mimeType);

    public static string GetSecureStoragePath(Guid ideaId, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var hashedName = $"{Guid.NewGuid():N}{extension}";
        return Path.Combine("upload_storage", "ideas", ideaId.ToString(), hashedName);
    }
}
