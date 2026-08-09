namespace BackendService.Services;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using BackendService.DTOs;

/// <summary>
/// HttpClient wrapper to communicate with the Python Flask ML microservice.
/// Sends order features + dynamic DB parameters, receives prediction result.
/// </summary>
public class MlApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MlApiService> _logger;

    public MlApiService(HttpClient httpClient, ILogger<MlApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Calls the Flask /predict endpoint with the given features and config.
    /// </summary>
    public async Task<FlaskPredictionResult?> PredictAsync(
        Dictionary<string, object> features,
        Dictionary<string, object> config)
    {
        var payload = new
        {
            features,
            config
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Calling Flask ML API /predict...");

        try
        {
            var response = await _httpClient.PostAsync("/predict", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Flask ML API returned non-success status code {StatusCode}: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"Flask ML API error ({(int)response.StatusCode}): {responseBody}");
            }

            var result = JsonSerializer.Deserialize<FlaskPredictionResult>(responseBody);

            _logger.LogInformation("Flask ML API returned: probability={Prob}, risk={Risk}",
                result?.probability, result?.risk_category);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Flask ML API endpoint at {BaseAddress}", _httpClient.BaseAddress);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Flask ML API request");
            throw;
        }
    }
}
