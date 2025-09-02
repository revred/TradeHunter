using CommandLine;
using Spectre.Console;

namespace TradeHunter;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Set up console for rich output
        AnsiConsole.Record();
        
        // Show beautiful banner
        ConsoleOutput.ShowBanner();
        
        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            ConsoleOutput.ShowGoodbye();
            Environment.Exit(0);
        };
        
        return await Parser.Default.ParseArguments<HuntOptions, ListOptions, ValidateOptions, McpOptions>(args)
            .MapResult<HuntOptions, ListOptions, ValidateOptions, McpOptions, Task<int>>(
                RunHunt,
                RunList,
                RunValidate,
                RunMcp,
                errs => Task.FromResult(1));
    }

    static async Task<int> RunHunt(HuntOptions options)
    {
        try
        {
            var loader = new StrategyLoader();
            var strategies = new List<TradingStrategy>();

            await ConsoleOutput.ShowProgress("Loading strategies", async console =>
            {
                await Task.Delay(500); // Simulate loading time

                if (!string.IsNullOrEmpty(options.StrategyFile))
                {
                    var strategy = await loader.LoadStrategyAsync(options.StrategyFile);
                    if (strategy != null)
                    {
                        strategies.Add(strategy);
                        ConsoleOutput.ShowStrategyLoading(strategy.Name, strategy.Symbol, strategy.Enabled);
                    }
                }
                else
                {
                    var loadedStrategies = await loader.LoadStrategiesAsync(options.StrategiesDirectory);
                    foreach (var strategy in loadedStrategies)
                    {
                        ConsoleOutput.ShowStrategyLoading(strategy.Name, strategy.Symbol, strategy.Enabled);
                    }
                    strategies.AddRange(loadedStrategies);
                }
            });

            if (strategies.Count == 0)
            {
                ConsoleOutput.ShowWarning("No strategies loaded. Exiting.");
                return 1;
            }

            // Show hunt session configuration
            ConsoleOutput.ShowHuntSession(strategies, options.DemoMode, "Development");
            
            ConsoleOutput.ShowInfo("Press Ctrl+C to stop hunting.");

            var engine = new ConfigurableHunterEngine(strategies, options);
            await engine.RunAsync();
            
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleOutput.ShowError("Hunt failed", ex);
            return 1;
        }
    }

    static async Task<int> RunList(ListOptions options)
    {
        try
        {
            var loader = new StrategyLoader();
            
            List<TradingStrategy> strategies = new();
            
            await ConsoleOutput.ShowProgress("Scanning strategies", async console =>
            {
                await Task.Delay(300); // Simulate loading time
                strategies = await loader.LoadStrategiesAsync(options.StrategiesDirectory);
            });

            ConsoleOutput.ShowStrategyList(strategies);
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleOutput.ShowError("Failed to list strategies", ex);
            return 1;
        }
    }

    static async Task<int> RunValidate(ValidateOptions options)
    {
        try
        {
            var loader = new StrategyLoader();
            var strategies = new List<TradingStrategy>();

            await ConsoleOutput.ShowProgress("Validating strategies", async console =>
            {
                await Task.Delay(400); // Simulate validation time

                if (!string.IsNullOrEmpty(options.StrategyFile))
                {
                    var strategy = await loader.LoadStrategyAsync(options.StrategyFile);
                    if (strategy != null)
                        strategies.Add(strategy);
                }
                else
                {
                    strategies = await loader.LoadStrategiesAsync(options.StrategiesDirectory);
                }
            });

            if (strategies.Count > 0)
            {
                ConsoleOutput.ShowSuccess($"Successfully validated {strategies.Count} strategies.");
                
                // Show validation details
                var table = new Table()
                    .Title("[bold green]✅ Validation Results[/]")
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Green);

                table.AddColumn("[bold yellow]Strategy[/]");
                table.AddColumn("[bold yellow]Symbol[/]");
                table.AddColumn("[bold yellow]Rules[/]");
                table.AddColumn("[bold yellow]Status[/]");

                foreach (var strategy in strategies)
                {
                    table.AddRow(
                        $"[white]{strategy.Name}[/]",
                        $"[yellow]{strategy.Symbol}[/]",
                        $"[blue]{strategy.EntryConditions.Count + strategy.ExitConditions.Count}[/]",
                        "[green]VALID[/]"
                    );
                }

                AnsiConsole.Write(table);
                return 0;
            }
            else
            {
                ConsoleOutput.ShowWarning("No valid strategies found.");
                return 1;
            }
        }
        catch (Exception ex)
        {
            ConsoleOutput.ShowError("Validation failed", ex);
            return 1;
        }
    }

    static async Task<int> RunMcp(McpOptions options)
    {
        try
        {
            ConsoleOutput.ShowMcpServerStartup(options.Port, options.StrategiesDirectory);
            
            var mcpServer = new McpServer(options);
            await mcpServer.RunAsync();
            
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleOutput.ShowError("MCP server failed to start", ex);
            return 1;
        }
    }
}
