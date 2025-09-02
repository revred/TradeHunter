using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeHunter;

public interface IDataFeed
{
    IAsyncEnumerable<Bar> StreamBarsAsync(string symbol, CancellationToken ct);
}

// Demo: synthetic intraday with a gap-up and momentum
public sealed class SimDataFeed : IDataFeed
{
    private readonly AppConfig _cfg;
    public SimDataFeed(AppConfig cfg) { _cfg = cfg; }

    public async IAsyncEnumerable<Bar> StreamBarsAsync(string symbol, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var today = DateTime.Today;
        var start = today.Add(_cfg.SessionOpen).AddMinutes(-60); // 60 premarket mins
        var rnd = new Random(42);

        decimal price = 1.90m;
        long volBase = 50_000;

        // Prev day (to have highs)
        var prevStart = today.AddDays(-1).Add(_cfg.SessionOpen);
        for (int i = 0; i < 390; i++) // 6.5 hours * 60
        {
            var ts = prevStart.AddMinutes(i);
            var drift = (decimal)(rnd.NextDouble() - 0.5) * 0.01m;
            price = Math.Max(1.5m, price * (1 + drift));
            var v = (long)(volBase * (0.5 + rnd.NextDouble()));
            yield return new Bar(ts, price, price * 1.004m, price * 0.996m, price, v);
            await Task.Delay(1, ct);
        }

        // Today premarket
        price *= 1.0m;
        for (int i = 0; i < 60; i++)
        {
            var ts = start.AddMinutes(i);
            var drift = (decimal)(rnd.NextDouble() - 0.5) * 0.005m;
            price = Math.Max(1.6m, price * (1 + drift));
            var v = (long)(volBase * 0.2 * (0.5 + rnd.NextDouble()));
            yield return new Bar(ts, price, price * 1.003m, price * 0.997m, price, v);
            await Task.Delay(1, ct);
        }

        // Gap up at open: +5%
        price *= 1.05m;
        var open = today.Add(_cfg.SessionOpen);
        for (int i = 0; i < 390; i++)
        {
            var ts = open.AddMinutes(i);
            var momentum = i < 60 ? 0.003m : 0.0005m; // strong first hour
            var noise = (decimal)(rnd.NextDouble() - 0.5) * 0.004m;
            price = Math.Max(1.7m, price * (1 + momentum + noise));
            var v = (long)(volBase * (i < 60 ? 4.0 : 1.2) * (0.5 + rnd.NextDouble()));
            yield return new Bar(ts, price, price * 1.006m, price * 0.994m, price, v);
            await Task.Delay(1, ct);
        }
    }
}

// CSV reader: file per day with columns: Ts,Open,High,Low,Close,Volume (minute bars).
// Path: ./data/{symbol}_YYYYMMDD.csv
public sealed class CsvDataFeed : IDataFeed
{
    private readonly AppConfig _cfg;
    public CsvDataFeed(AppConfig cfg) { _cfg = cfg; }

    public async IAsyncEnumerable<Bar> StreamBarsAsync(string symbol, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        // If no files, fall back to sim.
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        if (!Directory.Exists(dataDir))
        {
            var sim = new SimDataFeed(_cfg);
            await foreach (var b in sim.StreamBarsAsync(symbol, ct)) yield return b;
            yield break;
        }

        var files = Directory.GetFiles(dataDir, $"{symbol}_*.csv").OrderBy(f => f).ToList();
        if (files.Count == 0)
        {
            var sim = new SimDataFeed(_cfg);
            await foreach (var b in sim.StreamBarsAsync(symbol, ct)) yield return b;
            yield break;
        }

        foreach (var f in files)
        {
            using var sr = new StreamReader(f);
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                if (ct.IsCancellationRequested) yield break;
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split(',');
                if (parts.Length < 6) continue;
                var ts = DateTime.Parse(parts[0], null, DateTimeStyles.AssumeLocal);
                var o = decimal.Parse(parts[1]); var h = decimal.Parse(parts[2]); var l = decimal.Parse(parts[3]); var c = decimal.Parse(parts[4]);
                var v = long.Parse(parts[5]);
                yield return new Bar(ts, o, h, l, c, v);
                await Task.Delay(1, ct);
            }
        }
    }
}
