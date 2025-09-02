using System;
using System.Collections.Generic;

namespace TradeHunter;

public sealed class AppConfig
{
    // Core
    public string Symbol { get; set; } = "URG";
    public string VenueTz { get; set; } = "America/New_York";
    public bool DemoMode { get; set; } = true;

    // Risk (GBP-based user; convert to USD for share sizing)
    public decimal RiskBudgetGBP { get; set; } = 5000m;
    public decimal RiskPerAttemptGBP { get; set; } = 1250m;
    public decimal GBPUSD { get; set; } = 1.30m; // override via --fx=1.27 etc.

    // Trade windows (ET)
    public TimeSpan SessionOpen { get; set; } = new(9, 30, 0);
    public TimeSpan SessionClose { get; set; } = new(16, 0, 0);
    public int ORBMinutes { get; set; } = 15;

    // Rules
    public decimal MinGapPct { get; set; } = 0.03m; // 3% gap up over prior high
    public decimal MinRVOL { get; set; } = 2.0m;
    public bool RequireNews { get; set; } = true;
    public List<string> NewsKeywords { get; set; } = new() { "offtake", "contract", "DOE", "permit", "production", "uranium price", "spot jumps", "ramp", "award" };

    // Exits
    public decimal ATRMult { get; set; } = 1.5m;
    public decimal Scale1_R { get; set; } = 1.0m;
    public decimal Scale2_R { get; set; } = 2.0m;
    public bool EODTimeStop { get; set; } = true;

    public static AppConfig Default() => new();
}
