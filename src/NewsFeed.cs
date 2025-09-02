using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeHunter;

public interface INewsFeed
{
    Task<NewsItem?> TryGetLatestAsync(string symbol, DateTime now);
}

// Demo: injects a bullish headline near the open.
public sealed class SimNewsFeed : INewsFeed
{
    private readonly AppConfig _cfg;
    private bool _fired = false;
    public SimNewsFeed(AppConfig cfg) { _cfg = cfg; }

    public Task<NewsItem?> TryGetLatestAsync(string symbol, DateTime now)
    {
        var open = now.Date.Add(_cfg.SessionOpen);
        if (!_fired && now >= open.AddMinutes(5) && now <= open.AddMinutes(20))
        {
            _fired = true;
            return Task.FromResult<NewsItem?>(new(now, "UR-Energy announces multi-year offtake agreement; ramp accelerating at Lost Creek", "SimWire"));
        }
        return Task.FromResult<NewsItem?>(null);
    }
}

// Placeholder: always returns null. Replace with your news API connector.
public sealed class KeywordNewsFeed : INewsFeed
{
    private readonly AppConfig _cfg;
    public KeywordNewsFeed(AppConfig cfg) { _cfg = cfg; }
    public Task<NewsItem?> TryGetLatestAsync(string symbol, DateTime now) => Task.FromResult<NewsItem?>(null);
}
