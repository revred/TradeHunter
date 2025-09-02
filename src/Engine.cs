using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeHunter;

public sealed class OpeningRange
{
    public List<Bar> Bars { get; } = new();
    public bool IsComplete { get; set; }
    public decimal High => Bars.Count == 0 ? 0 : Bars.Max(b => b.High);
    public decimal Low  => Bars.Count == 0 ? 0 : Bars.Min(b => b.Low);
    public DateTime StartTs => Bars.FirstOrDefault()?.Ts ?? DateTime.MinValue;
}

public sealed class HunterEngine
{
    private readonly AppConfig _cfg;
    private readonly IDataFeed _data;
    private readonly INewsFeed _news;
    private readonly IExecutionBroker _broker;

    private readonly CompositeLongEntry _entryRule;
    private readonly Position _pos = new();
    private decimal _riskBudgetLeftGBP;
    private OpeningRange _orb = new();
    private List<Bar> _prevDay = new();
    private List<Bar> _today = new();
    private DateTime _sessionDate = DateTime.MinValue;
    private decimal _cumPV = 0m, _cumV = 0m; // for VWAP
    private Queue<long> _volWindow = new(); // for RVOL (simple intraday rolling baseline)

    public HunterEngine(AppConfig cfg, IDataFeed data, INewsFeed news, IExecutionBroker broker)
    {
        _cfg = cfg; _data = data; _news = news; _broker = broker;
        _entryRule = new CompositeLongEntry(new IRule[]{ new GapRule(), new ORBRule(), new VWAPRule(), new RVOLRule(), new NewsRule() });
        _riskBudgetLeftGBP = cfg.RiskBudgetGBP;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await foreach (var bar in _data.StreamBarsAsync(_cfg.Symbol, ct))
        {
            if (ct.IsCancellationRequested) break;
            if (_sessionDate != bar.Ts.Date)
            {
                // New session
                _prevDay = _today;
                _today = new();
                _orb = new();
                _cumPV = 0m; _cumV = 0m; _volWindow.Clear();
                _sessionDate = bar.Ts.Date;
                Console.WriteLine($"\n=== New Session {bar.Ts:yyyy-MM-dd} ===");
            }

            _today.Add(bar);
            _cumPV += bar.Close * bar.Volume;
            _cumV += bar.Volume;
            if (_volWindow.Count > 30) _volWindow.Dequeue();
            _volWindow.Enqueue(bar.Volume);

            var vwap = _cumV > 0 ? _cumPV / _cumV : bar.Close;
            var rvol = Utils.RVOL(bar.Volume, _volWindow);

            // Build ORB for first N minutes from session open
            var openTime = bar.Ts.Date.Add(_cfg.SessionOpen);
            if (bar.Ts < openTime.AddMinutes(_cfg.ORBMinutes)) _orb.Bars.Add(bar);
            else _orb.IsComplete = true;

            // Pull latest news if any
            var news = await _news.TryGetLatestAsync(_cfg.Symbol, bar.Ts);

            // Evaluate entry only if flat, in session
            var sessionCloseTs = bar.Ts.Date.Add(_cfg.SessionClose);
            if (!_pos.IsOpen && bar.Ts.TimeOfDay >= _cfg.SessionOpen && bar.Ts <= sessionCloseTs)
            {
                var (pass, checks) = _entryRule.Evaluate(bar, _today, _prevDay, news, _cfg, _orb, vwap, rvol);
                if (pass)
                {
                    var atr = Utils.ATR(_today, 5);
                    var stopByATR = bar.Close - _cfg.ATRMult * atr;
                    var stopByORL = _orb.IsComplete ? _orb.Low : bar.Low;
                    var stop = Math.Min(stopByATR, stopByORL);
                    var riskPerShare = Math.Max(0.01m, bar.Close - stop);
                    var qty = Sizer.ComputeShares(_cfg, riskPerShare, _riskBudgetLeftGBP);

                    if (qty > 0)
                    {
                        _pos.IsOpen = true;
                        _pos.Entry = bar.Close;
                        _pos.Stop = stop;
                        _pos.Quantity = qty;
                        _broker.Beep();
                        _broker.PlaceOrder(_cfg.Symbol, qty, bar.Close, stop, note: "LongEntry (Gap+ORB+VWAP+RVOL+News)");
                        _riskBudgetLeftGBP -= _cfg.RiskPerAttemptGBP;
                    }

                    LogChecks(bar, checks);
                }
            }
            else if (_pos.IsOpen)
            {
                // Scale logic
                var r = (bar.Close - _pos.Entry) / (_pos.RiskPerShare == 0 ? 1 : _pos.RiskPerShare);
                if (_pos.FilledScale1 == 0 && r >= _cfg.Scale1_R)
                {
                    var sell = _pos.Quantity / 3;
                    if (sell > 0) { _broker.SellPartial(_cfg.Symbol, sell, bar.Close, "Scale1"); _pos.FilledScale1 = sell; }
                }
                if (_pos.FilledScale2 == 0 && r >= _cfg.Scale2_R)
                {
                    var sell = _pos.Quantity / 3;
                    if (sell > 0) { _broker.SellPartial(_cfg.Symbol, sell, bar.Close, "Scale2"); _pos.FilledScale2 = sell; }
                }

                // Trailing via VWAP (simple): stop-up if price above vwap
                if (bar.Close > vwap && vwap > _pos.Stop) _pos.Stop = vwap;

                // Hard stop
                if (bar.Low <= _pos.Stop)
                {
                    var exitPx = _pos.Stop;
                    _broker.ExitAll(_cfg.Symbol, _pos.Quantity, exitPx, "StopHit");
                    _pos.IsOpen = false;
                }

                // EOD time stop
                if (_cfg.EODTimeStop && bar.Ts >= sessionCloseTs.AddMinutes(-1) && _pos.IsOpen)
                {
                    _broker.ExitAll(_cfg.Symbol, _pos.Quantity, bar.Close, "EOD Time Stop");
                    _pos.IsOpen = false;
                }
            }
        }
    }

    private static void LogChecks(Bar bar, List<RuleCheck> checks)
    {
        Console.WriteLine($"[{bar.Ts:HH:mm}] Entry criteria checks:");
        foreach (var c in checks)
            Console.WriteLine($"  • {c.Name}: {(c.Passed ? "PASS" : "FAIL")} — {c.Detail}");
    }
}
