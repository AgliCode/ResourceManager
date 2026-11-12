using System.Security.Cryptography;
using System.Text;

namespace ResourceManager.Core.Services;

public class ApiKeyStorageService
{
    private readonly string _filePath;

    public ApiKeyStorageService()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ResourceManager");

        Directory.CreateDirectory(folder);

        _filePath = Path.Combine(folder, "gemini-key.dat");
    }

    public void Save(string apiKey)
    {
        byte[] data = Encoding.UTF8.GetBytes(apiKey);

        byte[] encryptedData = ProtectedData.Protect(
            data,
            null,
            DataProtectionScope.CurrentUser);

        File.WriteAllBytes(_filePath, encryptedData);
    }

    public string? Load()
    {
        if (!File.Exists(_filePath))
            return null;

        byte[] encryptedData = File.ReadAllBytes(_filePath);

        byte[] decryptedData = ProtectedData.Unprotect(
            encryptedData,
            null,
            DataProtectionScope.CurrentUser);

        return Encoding.UTF8.GetString(decryptedData);
    }

    public void Delete()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }
    public bool Exists()
    {
        return File.Exists(_filePath);
    }
}