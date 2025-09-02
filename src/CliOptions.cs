using CommandLine;

namespace TradeHunter;

[Verb("hunt", HelpText = "Hunt for trades using configured strategies")]
public class HuntOptions
{
    [Option('s', "strategy", Required = false, HelpText = "Path to specific strategy file")]
    public string? StrategyFile { get; set; }

    [Option('d', "strategies-dir", Required = false, Default = "strategies", HelpText = "Directory containing strategy files")]
    public string StrategiesDirectory { get; set; } = "strategies";

    [Option("symbol", Required = false, HelpText = "Override symbol for strategy")]
    public string? Symbol { get; set; }

    [Option("demo", Required = false, Default = true, HelpText = "Run in demo mode")]
    public bool DemoMode { get; set; } = true;

    [Option("risk-budget", Required = false, HelpText = "Override risk budget")]
    public decimal? RiskBudget { get; set; }

    [Option("risk-per-attempt", Required = false, HelpText = "Override risk per attempt")]
    public decimal? RiskPerAttempt { get; set; }
}

[Verb("list", HelpText = "List available strategies")]
public class ListOptions
{
    [Option('d', "strategies-dir", Required = false, Default = "strategies", HelpText = "Directory containing strategy files")]
    public string StrategiesDirectory { get; set; } = "strategies";
}

[Verb("validate", HelpText = "Validate strategy files")]
public class ValidateOptions
{
    [Option('s', "strategy", Required = false, HelpText = "Path to specific strategy file to validate")]
    public string? StrategyFile { get; set; }

    [Option('d', "strategies-dir", Required = false, Default = "strategies", HelpText = "Directory containing strategy files")]
    public string StrategiesDirectory { get; set; } = "strategies";
}

[Verb("mcp", HelpText = "Run as MCP (Model Context Protocol) server")]
public class McpOptions
{
    [Option('p', "port", Required = false, Default = 3000, HelpText = "Port for MCP server")]
    public int Port { get; set; } = 3000;

    [Option('d', "strategies-dir", Required = false, Default = "strategies", HelpText = "Directory containing strategy files")]
    public string StrategiesDirectory { get; set; } = "strategies";
}