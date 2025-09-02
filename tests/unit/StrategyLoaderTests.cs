using Xunit;
using TradeHunter;
using System.IO;
using System.Threading.Tasks;

namespace TradeHunter.Tests.Unit;

public class StrategyLoaderTests
{
    private const string TestStrategyYaml = @"
name: ""Test Strategy""
description: ""Test strategy for unit testing""
symbol: ""TEST""
enabled: true

risk_management:
  budget_gbp: 1000
  risk_per_attempt_gbp: 100
  max_position_size_percent: 5
  stop_loss_percent: 3
  take_profit_percent: 8

entry_conditions:
  - type: ""price_momentum""
    parameters:
      timeframe: ""5m""
      momentum_threshold: 0.015

exit_conditions:
  - type: ""stop_loss""
    parameters:
      percent: 3

data_sources:
  price_feed: ""yahoo_finance""
  news_feed: ""news_api""
  
trading:
  mode: ""demo""
  broker: ""simulator""
";

    [Fact]
    public async Task LoadStrategyAsync_ValidYaml_ReturnsStrategy()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, TestStrategyYaml);
        var loader = new StrategyLoader();

        try
        {
            // Act
            var strategy = await loader.LoadStrategyAsync(tempFile);

            // Assert
            Assert.NotNull(strategy);
            Assert.Equal("Test Strategy", strategy.Name);
            Assert.Equal("TEST", strategy.Symbol);
            Assert.True(strategy.Enabled);
            Assert.Equal(1000m, strategy.RiskManagement.BudgetGbp);
            Assert.Single(strategy.EntryConditions);
            Assert.Single(strategy.ExitConditions);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadStrategyAsync_NonExistentFile_ReturnsNull()
    {
        // Arrange
        var loader = new StrategyLoader();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent-file.yml");

        // Act
        var strategy = await loader.LoadStrategyAsync(nonExistentFile);

        // Assert
        Assert.Null(strategy);
    }

    [Fact]
    public async Task LoadStrategiesAsync_EmptyDirectory_ReturnsEmptyList()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var loader = new StrategyLoader();

        try
        {
            // Act
            var strategies = await loader.LoadStrategiesAsync(tempDir);

            // Assert
            Assert.Empty(strategies);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public async Task LoadStrategiesAsync_WithValidStrategies_ReturnsEnabledOnly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        
        var enabledStrategyYaml = TestStrategyYaml;
        var disabledStrategyYaml = TestStrategyYaml.Replace("enabled: true", "enabled: false");

        var enabledFile = Path.Combine(tempDir, "enabled.yml");
        var disabledFile = Path.Combine(tempDir, "disabled.yml");

        await File.WriteAllTextAsync(enabledFile, enabledStrategyYaml);
        await File.WriteAllTextAsync(disabledFile, disabledStrategyYaml);

        var loader = new StrategyLoader();

        try
        {
            // Act
            var strategies = await loader.LoadStrategiesAsync(tempDir);

            // Assert
            Assert.Single(strategies);
            Assert.True(strategies[0].Enabled);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}