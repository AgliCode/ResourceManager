
using System.Diagnostics;


namespace ResourceManager.Core.Services;

public class ApiProcessManager : IDisposable
{
    private const string ApiUrl = "http://127.0.0.1:5177/";
    private Process? _apiProcess;

    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(2)
    };

    public async Task StartAsync(string apiDirectory)
    {
        if (await IsApiRunningAsync())
            return;

        string apiPath = Path.Combine(
            apiDirectory,
            "ResourceManager.TranslationApi.exe");

        if (!File.Exists(apiPath))
            throw new FileNotFoundException(
                "API executable was not found.",
                apiPath);

        _apiProcess = Process.Start(new ProcessStartInfo
        {
            FileName = apiPath,
            Arguments = "--urls http://127.0.0.1:5177",
            WorkingDirectory = apiDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        if (_apiProcess == null)
            throw new InvalidOperationException("Failed to start API.");

        for (int i = 0; i < 20; i++)
        {
            if (_apiProcess.HasExited)
                throw new InvalidOperationException(
                    "API process stopped unexpectedly.");

            if (await IsApiRunningAsync())
                return;

            await Task.Delay(500);
        }

        throw new TimeoutException(
            "API did not start within 10 seconds.");
    }

    private async Task<bool> IsApiRunningAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync(ApiUrl);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_apiProcess != null)
        {
            if (!_apiProcess.HasExited)
                _apiProcess.Kill(true);

            _apiProcess.Dispose();
        }

        _httpClient.Dispose();
    }
}
