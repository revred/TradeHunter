using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeHunter;

public static class Utils
{
    public static decimal ATR(IReadOnlyList<Bar> bars, int n)
    {
        if (bars.Count < n + 1) return Math.Max(0.02m, (bars.Last().High - bars.Last().Low) * 0.5m);
        decimal sum = 0;
        for (int i = bars.Count - n; i < bars.Count; i++)
        {
            var h = bars[i].High;
            var l = bars[i].Low;
            var cPrev = bars[i - 1].Close;
            var tr = Math.Max(h - l, Math.Max(Math.Abs(h - cPrev), Math.Abs(l - cPrev)));
            sum += tr;
        }
        return sum / n;
    }

    public static decimal RVOL(long vol, IEnumerable<long> window)
    {
        var arr = window.ToArray();
        if (arr.Length < 5) return 1m;
        var avg = arr.Average();
        return avg <= 0 ? 1m : (decimal)vol / (decimal)avg;
    }
}

public static class Sizer
{
    public static int ComputeShares(AppConfig cfg, decimal riskPerShareUSD, decimal riskBudgetLeftGBP)
    {
        var riskPerAttemptGBP = Math.Min(cfg.RiskPerAttemptGBP, riskBudgetLeftGBP);
        var riskPerAttemptUSD = riskPerAttemptGBP * cfg.GBPUSD;
        if (riskPerShareUSD <= 0) return 0;
        var qty = (int)(riskPerAttemptUSD / riskPerShareUSD);
        return Math.Max(0, qty);
    }
}
