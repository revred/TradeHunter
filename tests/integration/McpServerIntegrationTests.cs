using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using TradeHunter;

namespace TradeHunter.Tests.Integration;

public class McpServerIntegrationTests : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly int _testPort = 3001; // Use different port for testing

    public McpServerIntegrationTests()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_testPort}/");
    }

    [Fact(Skip = "Integration test - requires running MCP server")]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        // Act
        var response = await _httpClient.GetAsync("health");
        var content = await response.Content.ReadAsStringAsync();
        var healthData = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("healthy", healthData.GetProperty("status").GetString());
        Assert.Equal("1.0.0", healthData.GetProperty("version").GetString());
    }

    [Fact(Skip = "Integration test - requires running MCP server")]
    public async Task StrategiesEndpoint_ReturnsStrategyList()
    {
        // Act
        var response = await _httpClient.GetAsync("strategies");
        var content = await response.Content.ReadAsStringAsync();
        var strategiesData = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(strategiesData.TryGetProperty("strategies", out var strategies));
        Assert.True(strategies.GetArrayLength() >= 0);
    }

    [Fact(Skip = "Integration test - requires running MCP server")]
    public async Task ValidateEndpoint_ReturnsValidationResults()
    {
        // Act
        var response = await _httpClient.GetAsync("validate");
        var content = await response.Content.ReadAsStringAsync();
        var validationData = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(validationData.TryGetProperty("validation", out var validation));
    }

    [Fact(Skip = "Integration test - requires running MCP server")]
    public async Task HuntEndpoint_AcceptsPostRequest()
    {
        // Arrange
        var huntRequest = new
        {
            demoMode = true,
            symbol = "TEST",
            riskBudget = 1000
        };

        var json = JsonSerializer.Serialize(huntRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("hunt", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("Hunt started successfully", responseContent);
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        await Task.CompletedTask;
    }
}

public class McpServerTestFixture : IAsyncDisposable
{
    private Task? _serverTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public async Task StartServerAsync(int port = 3001)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        var options = new McpOptions 
        { 
            Port = port,
            StrategiesDirectory = "config/strategies"
        };
        
        var server = new McpServer(options);
        
        _serverTask = Task.Run(async () => 
        {
            try 
            {
                await server.RunAsync();
            }
            catch (Exception ex) when (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Expected when stopping
            }
        });

        // Give server time to start
        await Task.Delay(2000);
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource?.Cancel();
        
        if (_serverTask != null)
        {
            await _serverTask;
        }

        _cancellationTokenSource?.Dispose();
    }
}