using System;
using System.Collections.Generic;

namespace TradeHunter;

public record Bar(DateTime Ts, decimal Open, decimal High, decimal Low, decimal Close, long Volume);
public record NewsItem(DateTime Ts, string Headline, string Source);
public record RuleCheck(string Name, bool Passed, string Detail);

public enum SignalKind { None, LongEntry, ScaleOut1, ScaleOut2, ExitAll }

public sealed class Signal
{
    public SignalKind Kind { get; init; } = SignalKind.None;
    public DateTime Ts { get; init; }
    public decimal Price { get; init; }
    public string Note { get; init; } = "";
    public List<RuleCheck> Checks { get; init; } = new();
}

public sealed class Position
{
    public bool IsOpen { get; set; }
    public decimal Entry { get; set; }
    public int Quantity { get; set; }
    public decimal Stop { get; set; }
    public decimal RiskPerShare => Entry - Stop;
    public decimal RealizedPnl { get; set; }
    public int FilledScale1 { get; set; }
    public int FilledScale2 { get; set; }
}
