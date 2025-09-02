using Spectre.Console;
using Spectre.Console.Rendering;

namespace TradeHunter;

public static class ConsoleOutput
{
    /// <summary>
    /// Display the TradeHunter ASCII art banner with rich colors
    /// </summary>
    public static void ShowBanner()
    {
        var banner = new FigletText("TradeHunter")
            .Color(Color.Cyan1);

        AnsiConsole.Write(banner);

        var subtitle = new Markup("[bold green]Configurable Trading Strategy Engine[/] [dim yellow]v2.0[/]")
            .Centered();

        AnsiConsole.Write(subtitle);
        AnsiConsole.WriteLine();
        
        var gradient = new Rule("[bold blue]Hunt • Analyze • Execute • Profit[/]")
            .RuleStyle(Style.Parse("dim blue"));
        
        AnsiConsole.Write(gradient);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display strategy loading progress with rich formatting
    /// </summary>
    public static void ShowStrategyLoading(string strategyName, string symbol, bool enabled)
    {
        var statusIcon = enabled ? ":check_mark:" : ":cross_mark:";
        var statusColor = enabled ? "green" : "red";
        var enabledText = enabled ? "ENABLED" : "DISABLED";

        AnsiConsole.MarkupLine($"{statusIcon} [bold white]{strategyName}[/] ([yellow]{symbol}[/]) - [bold {statusColor}]{enabledText}[/]");
    }

    /// <summary>
    /// Display hunt session information with rich table
    /// </summary>
    public static void ShowHuntSession(List<TradingStrategy> strategies, bool demoMode, string environment)
    {
        var table = new Table()
            .Title("[bold cyan]🎯 Hunt Session Configuration[/]")
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1);

        table.AddColumn(new TableColumn("[bold yellow]Strategy[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Symbol[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Risk Budget[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Mode[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Status[/]").Centered());

        foreach (var strategy in strategies)
        {
            var modeIcon = demoMode ? ":test_tube:" : ":money_with_wings:";
            var modeText = demoMode ? "[green]DEMO[/]" : "[red]LIVE[/]";
            var statusIcon = strategy.Enabled ? ":green_circle:" : ":red_circle:";

            table.AddRow(
                $"[bold white]{strategy.Name}[/]",
                $"[yellow]{strategy.Symbol}[/]",
                $"[green]£{strategy.RiskManagement.BudgetGbp:N0}[/]",
                $"{modeIcon} {modeText}",
                $"{statusIcon} Ready"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var envPanel = new Panel($"[bold green]Environment:[/] [yellow]{environment.ToUpper()}[/]")
            .Header("[bold blue]🔧 Configuration[/]")
            .BorderColor(Color.Blue)
            .Padding(1, 0);

        AnsiConsole.Write(envPanel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display market session information
    /// </summary>
    public static void ShowMarketSession(DateTime sessionDate, string symbol)
    {
        var datePanel = new Panel($"[bold white]{sessionDate:yyyy-MM-dd}[/]")
            .Header($"[bold green]📅 New Trading Session - {symbol}[/]")
            .BorderColor(Color.Green)
            .Expand();

        AnsiConsole.Write(datePanel);
    }

    /// <summary>
    /// Display trade execution with rich formatting
    /// </summary>
    public static void ShowTradeExecution(string action, string symbol, int quantity, decimal price, decimal? stop = null, string? note = null)
    {
        var actionColor = action.ToUpper() switch
        {
            "BUY" => "green",
            "SELL" => "red",
            "EXIT" => "orange1",
            _ => "white"
        };

        var actionIcon = action.ToUpper() switch
        {
            "BUY" => ":chart_with_upwards_trend:",
            "SELL" => ":chart_with_downwards_trend:",
            "EXIT" => ":cross_mark:",
            _ => ":gear:"
        };

        var tradeInfo = $"{actionIcon} [bold {actionColor}]{action.ToUpper()}[/] [yellow]{symbol}[/] x[bold white]{quantity}[/] @ [green]${price:F2}[/]";

        if (stop.HasValue)
        {
            tradeInfo += $" | Stop: [red]${stop.Value:F2}[/]";
        }

        if (!string.IsNullOrEmpty(note))
        {
            tradeInfo += $" | [dim]{note}[/]";
        }

        var panel = new Panel(tradeInfo)
            .Header("[bold blue]💼 Trade Execution[/]")
            .BorderColor(Color.Blue)
            .Padding(1, 0);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display rule checks with detailed formatting
    /// </summary>
    public static void ShowRuleChecks(DateTime timestamp, string symbol, List<RuleCheck> checks)
    {
        var table = new Table()
            .Title($"[bold cyan]🔍 Entry Criteria Analysis - {symbol} [{timestamp:HH:mm:ss}][/]")
            .Border(TableBorder.Simple)
            .BorderColor(Color.Cyan1);

        table.AddColumn(new TableColumn("[bold yellow]Rule[/]"));
        table.AddColumn(new TableColumn("[bold yellow]Status[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Detail[/]"));

        foreach (var check in checks)
        {
            var statusIcon = check.Passed ? ":check_mark:" : ":cross_mark:";
            var statusColor = check.Passed ? "green" : "red";
            var statusText = check.Passed ? "PASS" : "FAIL";

            table.AddRow(
                $"[bold white]{check.Name}[/]",
                $"{statusIcon} [bold {statusColor}]{statusText}[/]",
                $"[dim]{check.Detail}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display MCP server startup information
    /// </summary>
    public static void ShowMcpServerStartup(int port, string strategiesDirectory)
    {
        var serverInfo = new Panel(
            $"[bold green]Port:[/] [yellow]{port}[/]\n" +
            $"[bold green]Strategies:[/] [yellow]{strategiesDirectory}[/]\n" +
            $"[bold green]Status:[/] [green]READY[/]")
            .Header("[bold blue]🌐 MCP Server[/]")
            .BorderColor(Color.Blue)
            .Expand();

        AnsiConsole.Write(serverInfo);

        var endpoints = new Table()
            .Title("[bold cyan]🛠️  API Endpoints[/]")
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1);

        endpoints.AddColumn(new TableColumn("[bold yellow]Endpoint[/]"));
        endpoints.AddColumn(new TableColumn("[bold yellow]Method[/]"));
        endpoints.AddColumn(new TableColumn("[bold yellow]Description[/]"));

        endpoints.AddRow("[green]/health[/]", "[blue]GET[/]", "Server health check");
        endpoints.AddRow("[green]/strategies[/]", "[blue]GET[/]", "List available strategies");
        endpoints.AddRow("[green]/hunt[/]", "[orange1]POST[/]", "Start hunting for trades");
        endpoints.AddRow("[green]/validate[/]", "[blue]GET[/]", "Validate strategy configurations");

        AnsiConsole.Write(endpoints);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold green]🚀 Server ready for requests![/]");
    }

    /// <summary>
    /// Display error messages with consistent formatting
    /// </summary>
    public static void ShowError(string message, Exception? exception = null)
    {
        var errorPanel = new Panel($"[red]{message}[/]")
            .Header("[bold red]❌ Error[/]")
            .BorderColor(Color.Red);

        AnsiConsole.Write(errorPanel);

        if (exception != null)
        {
            AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything);
        }
    }

    /// <summary>
    /// Display success messages
    /// </summary>
    public static void ShowSuccess(string message)
    {
        var successPanel = new Panel($"[green]{message}[/]")
            .Header("[bold green]✅ Success[/]")
            .BorderColor(Color.Green);

        AnsiConsole.Write(successPanel);
    }

    /// <summary>
    /// Display warning messages
    /// </summary>
    public static void ShowWarning(string message)
    {
        var warningPanel = new Panel($"[yellow]{message}[/]")
            .Header("[bold yellow]⚠️  Warning[/]")
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(warningPanel);
    }

    /// <summary>
    /// Display information messages
    /// </summary>
    public static void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[bold blue]ℹ️  [/][dim]{message}[/]");
    }

    /// <summary>
    /// Show progress bar for operations
    /// </summary>
    public static async Task ShowProgress(string taskName, Func<IAnsiConsole, Task> operation)
    {
        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[bold blue]{taskName}[/]");
                
                await operation(AnsiConsole.Console);
                
                task.Increment(100);
            });
    }

    /// <summary>
    /// Display strategy list in a beautiful table
    /// </summary>
    public static void ShowStrategyList(List<TradingStrategy> strategies)
    {
        if (!strategies.Any())
        {
            ShowWarning("No strategies found in the specified directory.");
            return;
        }

        var table = new Table()
            .Title($"[bold cyan]📊 Available Strategies ({strategies.Count})[/]")
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1);

        table.AddColumn(new TableColumn("[bold yellow]Name[/]"));
        table.AddColumn(new TableColumn("[bold yellow]Symbol[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Budget[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold yellow]Entry Rules[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Exit Rules[/]").Centered());
        table.AddColumn(new TableColumn("[bold yellow]Status[/]").Centered());

        foreach (var strategy in strategies)
        {
            var statusIcon = strategy.Enabled ? ":check_mark:" : ":cross_mark:";
            var statusColor = strategy.Enabled ? "green" : "red";
            var statusText = strategy.Enabled ? "ENABLED" : "DISABLED";

            table.AddRow(
                $"[bold white]{strategy.Name}[/]",
                $"[yellow]{strategy.Symbol}[/]",
                $"[green]£{strategy.RiskManagement.BudgetGbp:N0}[/]",
                $"[blue]{strategy.EntryConditions.Count}[/]",
                $"[orange1]{strategy.ExitConditions.Count}[/]",
                $"{statusIcon} [bold {statusColor}]{statusText}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Show live status updates during hunting
    /// </summary>
    public static void ShowLiveStatus(string symbol, decimal price, decimal volume, string status)
    {
        var statusColor = status.ToLower() switch
        {
            "scanning" => "blue",
            "opportunity" => "green",
            "risk" => "red",
            _ => "white"
        };

        AnsiConsole.MarkupLine($"[dim gray]{DateTime.Now:HH:mm:ss}[/] | [yellow]{symbol}[/] | [white]${price:F2}[/] | Vol: [blue]{volume:N0}[/] | [{statusColor}]{status.ToUpper()}[/]");
    }

    /// <summary>
    /// Display application exit message
    /// </summary>
    public static void ShowGoodbye()
    {
        var goodbye = new Panel("[bold green]Happy hunting! 🎯[/]")
            .Header("[bold blue]👋 TradeHunter[/]")
            .BorderColor(Color.Blue)
            .Padding(1, 0);

        AnsiConsole.Write(goodbye);
    }
}