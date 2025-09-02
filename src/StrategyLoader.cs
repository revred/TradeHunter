using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TradeHunter;

public class StrategyLoader
{
    private readonly IDeserializer _deserializer;

    public StrategyLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    public async Task<List<TradingStrategy>> LoadStrategiesAsync(string strategiesPath = "strategies")
    {
        var strategies = new List<TradingStrategy>();
        
        if (!Directory.Exists(strategiesPath))
        {
            ConsoleOutput.ShowWarning($"Strategies directory '{strategiesPath}' not found.");
            return strategies;
        }

        var yamlFiles = Directory.GetFiles(strategiesPath, "*.yml")
            .Concat(Directory.GetFiles(strategiesPath, "*.yaml"));

        foreach (var file in yamlFiles)
        {
            try
            {
                var yaml = await File.ReadAllTextAsync(file);
                var strategy = _deserializer.Deserialize<TradingStrategy>(yaml);
                
                strategies.Add(strategy); // Add both enabled and disabled for display
            }
            catch (Exception ex)
            {
                ConsoleOutput.ShowError($"Failed to load strategy from {file}", ex);
            }
        }

        return strategies;
    }

    public async Task<TradingStrategy?> LoadStrategyAsync(string filePath)
    {
        try
        {
            var yaml = await File.ReadAllTextAsync(filePath);
            return _deserializer.Deserialize<TradingStrategy>(yaml);
        }
        catch (Exception ex)
        {
            ConsoleOutput.ShowError($"Failed to load strategy from {filePath}", ex);
            return null;
        }
    }
}