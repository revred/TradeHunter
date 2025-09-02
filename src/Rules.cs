using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeHunter;

public interface IRule
{
    RuleCheck Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol);
}

public sealed class GapRule : IRule
{
    public RuleCheck Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol)
    {
        if (prevDayBars.Count == 0) return new("GapRule", false, "No prior day.");
        var prevHigh = prevDayBars.Max(b => b.High);
        var passed = current.Open >= prevHigh * (1 + cfg.MinGapPct);
        return new("GapRule", passed, $"Open={current.Open:F2} vs PrevHigh={prevHigh:F2}, minGap={cfg.MinGapPct:P0}");
    }
}

public sealed class ORBRule : IRule
{
    public RuleCheck Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol)
    {
        var afterORB = orng.IsComplete && dayBars.Count > orng.Bars.Count;
        var brokeHigh = afterORB && current.High >= orng.High;
        return new("ORBRule", brokeHigh, $"ORBHigh={orng.High:F2}, afterORB={afterORB}");
    }
}

public sealed class VWAPRule : IRule
{
    public RuleCheck Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol)
    {
        var above = current.Close >= vwap;
        return new("VWAPRule", above, $"Price={current.Close:F2} vs VWAP={vwap:F2}");
    }
}

public sealed class RVOLRule : IRule
{
    public RuleCheck Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol)
    {
        var pass = rvol >= cfg.MinRVOL;
        return new("RVOLRule", pass, $"RVOL={rvol:F2} (min {cfg.MinRVOL:F2})");
    }
}

public sealed class NewsRule : IRule
{
    public RuleCheck Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol)
    {
        if (!cfg.RequireNews) return new("NewsRule", true, "News not required.");
        if (latestNews is null) return new("NewsRule", false, "No fresh news.");
        var headline = latestNews.Headline.ToLowerInvariant();
        var hit = cfg.NewsKeywords.Any(k => headline.Contains(k.ToLowerInvariant()));
        return new("NewsRule", hit, hit ? $"Hit: \"{latestNews.Headline}\"" : $"No keyword hit in: \"{latestNews.Headline}\"");
    }
}

public sealed class CompositeLongEntry
{
    private readonly List<IRule> _rules;
    public CompositeLongEntry(IEnumerable<IRule> rules) => _rules = rules.ToList();

    public (bool pass, List<RuleCheck> checks) Evaluate(Bar current, IReadOnlyList<Bar> dayBars, IReadOnlyList<Bar> prevDayBars, NewsItem? latestNews, AppConfig cfg, OpeningRange orng, decimal vwap, decimal rvol)
    {
        var checks = new List<RuleCheck>();
        var pass = true;
        foreach (var r in _rules)
        {
            var chk = r.Evaluate(current, dayBars, prevDayBars, latestNews, cfg, orng, vwap, rvol);
            checks.Add(chk);
            if (!chk.Passed) pass = false;
        }
        return (pass, checks);
    }
}
