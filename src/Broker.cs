using System;

namespace TradeHunter;

public interface IExecutionBroker
{
    void PlaceOrder(string symbol, int qty, decimal price, decimal stop, string note);
    void SellPartial(string symbol, int qty, decimal price, string note);
    void ExitAll(string symbol, int qty, decimal price, string note);
    void Beep();
}

public sealed class SimBroker : IExecutionBroker
{
    private readonly AppConfig _cfg;
    public SimBroker(AppConfig cfg) { _cfg = cfg; }

    public void PlaceOrder(string symbol, int qty, decimal price, decimal stop, string note)
    {
        Console.WriteLine($"[BROKER] BUY {symbol} x{qty} @ {price:F2}, Stop {stop:F2} — {note}");
    }

    public void SellPartial(string symbol, int qty, decimal price, string note)
    {
        Console.WriteLine($"[BROKER] SELL-PART {symbol} x{qty} @ {price:F2} — {note}");
    }

    public void ExitAll(string symbol, int qty, decimal price, string note)
    {
        Console.WriteLine($"[BROKER] EXIT-ALL {symbol} x{qty} @ {price:F2} — {note}");
    }

    public void Beep()
    {
        try { Console.Beep(); } catch { /* non-Windows */ }
    }
}
