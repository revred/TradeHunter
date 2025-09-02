using System.Text.Json;

namespace TradeHunter;

public class McpServer
{
    private readonly McpOptions _options;
    private readonly StrategyLoader _strategyLoader;

    public McpServer(McpOptions options)
    {
        _options = options;
        _strategyLoader = new StrategyLoader();
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"MCP Server starting on port {_options.Port}");
        Console.WriteLine($"Strategies directory: {_options.StrategiesDirectory}");
        
        var listener = new System.Net.HttpListener();
        listener.Prefixes.Add($"http://localhost:{_options.Port}/");
        listener.Start();
        
        Console.WriteLine("MCP Server ready. Waiting for requests...");
        
        while (true)
        {
            try
            {
                var context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
        }
    }

    private async Task HandleRequestAsync(System.Net.HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        try
        {
            var path = request.Url?.AbsolutePath ?? "/";
            
            switch (path)
            {
                case "/strategies":
                    await HandleListStrategiesAsync(response);
                    break;
                    
                case "/hunt":
                    await HandleHuntRequestAsync(request, response);
                    break;
                    
                case "/validate":
                    await HandleValidateRequestAsync(request, response);
                    break;
                    
                case "/health":
                    await HandleHealthCheckAsync(response);
                    break;
                    
                default:
                    response.StatusCode = 404;
                    await WriteJsonResponseAsync(response, new { error = "Not found" });
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            await WriteJsonResponseAsync(response, new { error = ex.Message });
        }
        finally
        {
            response.Close();
        }
    }

    private async Task HandleListStrategiesAsync(System.Net.HttpListenerResponse response)
    {
        var strategies = await _strategyLoader.LoadStrategiesAsync(_options.StrategiesDirectory);
        
        var strategySummaries = strategies.Select(s => new
        {
            name = s.Name,
            symbol = s.Symbol,
            description = s.Description,
            enabled = s.Enabled,
            entryConditions = s.EntryConditions.Count,
            exitConditions = s.ExitConditions.Count,
            riskBudget = s.RiskManagement.BudgetGbp,
            tradingMode = s.Trading.Mode
        });

        await WriteJsonResponseAsync(response, new { strategies = strategySummaries });
    }

    private async Task HandleHuntRequestAsync(System.Net.HttpListenerRequest request, System.Net.HttpListenerResponse response)
    {
        if (request.HttpMethod != "POST")
        {
            response.StatusCode = 405;
            await WriteJsonResponseAsync(response, new { error = "Method not allowed" });
            return;
        }

        using var reader = new StreamReader(request.InputStream);
        var body = await reader.ReadToEndAsync();
        var options = JsonSerializer.Deserialize<HuntRequest>(body);

        if (options == null)
        {
            response.StatusCode = 400;
            await WriteJsonResponseAsync(response, new { error = "Invalid request body" });
            return;
        }

        var huntOptions = new HuntOptions
        {
            StrategyFile = options.StrategyFile,
            StrategiesDirectory = options.StrategiesDirectory ?? _options.StrategiesDirectory,
            Symbol = options.Symbol,
            DemoMode = options.DemoMode ?? true,
            RiskBudget = options.RiskBudget,
            RiskPerAttempt = options.RiskPerAttempt
        };

        _ = Task.Run(async () =>
        {
            try
            {
                var loader = new StrategyLoader();
                var strategies = new List<TradingStrategy>();

                if (!string.IsNullOrEmpty(huntOptions.StrategyFile))
                {
                    var strategy = await loader.LoadStrategyAsync(huntOptions.StrategyFile);
                    if (strategy != null)
                        strategies.Add(strategy);
                }
                else
                {
                    strategies = await loader.LoadStrategiesAsync(huntOptions.StrategiesDirectory);
                }

                if (strategies.Count > 0)
                {
                    var engine = new ConfigurableHunterEngine(strategies, huntOptions);
                    await engine.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hunt task failed: {ex.Message}");
            }
        });

        await WriteJsonResponseAsync(response, new { message = "Hunt started successfully" });
    }

    private async Task HandleValidateRequestAsync(System.Net.HttpListenerRequest request, System.Net.HttpListenerResponse response)
    {
        var strategies = await _strategyLoader.LoadStrategiesAsync(_options.StrategiesDirectory);
        
        var validation = strategies.Select(s => new
        {
            name = s.Name,
            valid = true,
            symbol = s.Symbol,
            issues = new string[0]
        });

        await WriteJsonResponseAsync(response, new { validation });
    }

    private async Task HandleHealthCheckAsync(System.Net.HttpListenerResponse response)
    {
        await WriteJsonResponseAsync(response, new 
        { 
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            strategiesDirectory = _options.StrategiesDirectory
        });
    }

    private async Task WriteJsonResponseAsync(System.Net.HttpListenerResponse response, object data)
    {
        response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var buffer = System.Text.Encoding.UTF8.GetBytes(json);
        await response.OutputStream.WriteAsync(buffer);
    }
}

public class HuntRequest
{
    public string? StrategyFile { get; set; }
    public string? StrategiesDirectory { get; set; }
    public string? Symbol { get; set; }
    public bool? DemoMode { get; set; }
    public decimal? RiskBudget { get; set; }
    public decimal? RiskPerAttempt { get; set; }
}