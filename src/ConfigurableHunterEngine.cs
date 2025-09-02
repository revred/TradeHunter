namespace TradeHunter;

public class ConfigurableHunterEngine
{
    private readonly List<TradingStrategy> _strategies;
    private readonly HuntOptions _options;

    public ConfigurableHunterEngine(List<TradingStrategy> strategies, HuntOptions options)
    {
        _strategies = strategies;
        _options = options;
    }

    public async Task RunAsync()
    {
        var cancellationToken = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationToken.Cancel();
        };

        var tasks = _strategies.Select(strategy => RunStrategyAsync(strategy, cancellationToken.Token));
        
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nShutdown requested. Stopping all strategies...");
        }
    }

    private async Task RunStrategyAsync(TradingStrategy strategy, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting strategy: {strategy.Name} for {strategy.Symbol}");
        
        var config = CreateAppConfigFromStrategy(strategy);
        ApplyCliOverrides(config);

        var dataFeed = CreateDataFeed(strategy, config);
        var newsFeed = CreateNewsFeed(strategy, config);
        var broker = CreateBroker(strategy, config);
        
        var engine = new HunterEngine(config, dataFeed, newsFeed, broker);
        
        try
        {
            await engine.RunAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Strategy {strategy.Name} stopped.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Strategy {strategy.Name} failed: {ex.Message}");
        }
    }

    private AppConfig CreateAppConfigFromStrategy(TradingStrategy strategy)
    {
        var config = AppConfig.Default();
        
        config.Symbol = strategy.Symbol;
        config.RiskBudgetGBP = strategy.RiskManagement.BudgetGbp;
        config.RiskPerAttemptGBP = strategy.RiskManagement.RiskPerAttemptGbp;
        config.DemoMode = strategy.Trading.Mode.Equals("demo", StringComparison.OrdinalIgnoreCase);

        return config;
    }

    private void ApplyCliOverrides(AppConfig config)
    {
        if (!string.IsNullOrEmpty(_options.Symbol))
            config.Symbol = _options.Symbol.ToUpperInvariant();
        
        if (_options.RiskBudget.HasValue)
            config.RiskBudgetGBP = _options.RiskBudget.Value;
        
        if (_options.RiskPerAttempt.HasValue)
            config.RiskPerAttemptGBP = _options.RiskPerAttempt.Value;

        config.DemoMode = _options.DemoMode;
    }

    private IDataFeed CreateDataFeed(TradingStrategy strategy, AppConfig config)
    {
        return strategy.DataSources.PriceFeed.ToLowerInvariant() switch
        {
            "yahoo_finance" => config.DemoMode ? new SimDataFeed(config) : new CsvDataFeed(config),
            "alpha_vantage" => config.DemoMode ? new SimDataFeed(config) : new CsvDataFeed(config),
            _ => config.DemoMode ? new SimDataFeed(config) : new CsvDataFeed(config)
        };
    }

    private INewsFeed CreateNewsFeed(TradingStrategy strategy, AppConfig config)
    {
        return strategy.DataSources.NewsFeed.ToLowerInvariant() switch
        {
            "financial_news_api" => config.DemoMode ? new SimNewsFeed(config) : new KeywordNewsFeed(config),
            "news_api" => config.DemoMode ? new SimNewsFeed(config) : new KeywordNewsFeed(config),
            _ => config.DemoMode ? new SimNewsFeed(config) : new KeywordNewsFeed(config)
        };
    }

    private IExecutionBroker CreateBroker(TradingStrategy strategy, AppConfig config)
    {
        return strategy.Trading.Broker.ToLowerInvariant() switch
        {
            "simulator" => new SimBroker(config),
            _ => new SimBroker(config)
        };
    }
}