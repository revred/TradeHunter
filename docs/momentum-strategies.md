# 🚀 Momentum Trading Strategies for Explosive Stocks

This document covers the specialized momentum strategies designed for high-volatility, news-driven stocks with explosive potential.

## 📈 Strategy Overview

### Core Philosophy: **"Momentum is Everything, News is the Source"**

These strategies are built around three fundamental principles:

1. **📊 VOLUME CONFIRMS EVERYTHING** - No volume, no trade
2. **📰 NEWS DRIVES MOMENTUM** - Fresh catalysts are essential  
3. **🌊 RIDE THE WAVE** - Don't fight momentum, embrace it

## 🎯 Target Symbols & Categories

### 🧬 Biotech/Pharma Catalysts
**Symbols:** SLRX, BREA, LICN, AKAN, NDRA, CNSP, UPC, SLGL, TIVC

**Key Drivers:**
- FDA approvals and breakthrough therapy designations
- Clinical trial data releases (Phase II/III results)
- Partnership announcements with big pharma
- Patent approvals and orphan drug status
- Regulatory milestone achievements

**Strategy:** `biotech-catalyst-hunter.yml`
- Ultra-conservative position sizing (1.5% max)
- Pre-market monitoring (biotech news drops early)
- Staged profit taking (15%, 25%, 35% targets)
- Binary event protection

### 🇨🇳 China ADR Momentum  
**Symbols:** CHNR, NAAS, BAOS

**Key Drivers:**
- China policy announcements and stimulus measures
- Regulatory clarity and crackdown reversals
- Trade deal progress and tariff reductions
- Economic reopening and GDP growth
- Sector-specific government support

**Strategy:** `china-adr-momentum.yml`
- Overnight gap analysis (China trades while US sleeps)
- Correlation with FXI, KWEB, ASHR ETFs
- Yuan exchange rate factor analysis
- Weekend policy risk management

### 🔥 Low Float Squeeze Plays
**Symbols:** SUGP, FGI, JFBR, MDRR, CISS, BBGI, LDWY, NAOV, PRTG, CLRO

**Key Drivers:**
- Massive volume spikes (10-20x normal)
- High short interest with low float
- Social media buzz and retail interest
- Earnings surprises and guidance raises
- Acquisition rumors and takeover speculation

**Strategy:** `low-float-squeeze.yml`
- Nuclear volume detection (20x average)
- Short interest and days-to-cover analysis
- Social sentiment monitoring (StockTwits, Reddit)
- Intraday-only trades (no overnight holds)

## 📋 Strategy Configuration Files

### 1. Master Momentum Hunter (`master-momentum-hunter.yml`)
**The Swiss Army Knife** - Adaptively hunts all 24 symbols with intelligent categorization:

```yaml
# Scans all symbols every 5 minutes
# Ranks by momentum score (volume 40%, price 30%, news 20%, technicals 10%)
# Trades top 3 ranked symbols simultaneously
# Adaptive risk based on symbol category
```

### 2. Small-Cap Momentum Hunter (`small-cap-momentum-hunter.yml`)
**The Volume Detective** - Focuses on volume explosions with news confirmation:

```yaml
# 5x volume minimum with news catalyst required
# Wide stops (8%) with high targets (20%)
# Market regime filtering (avoid VIX >35)
# Symbol-specific keyword optimization
```

### 3. Biotech Catalyst Hunter (`biotech-catalyst-hunter.yml`)
**The FDA Tracker** - Specialized for binary biotech events:

```yaml
# 10x volume explosion detection
# FDA/clinical trial news parsing
# Pre-market gap and halt monitoring
# Staged profit taking (15%, 25%, 35%)
```

### 4. China ADR Momentum (`china-adr-momentum.yml`)
**The Overnight Gap Master** - Captures policy-driven momentum:

```yaml
# Shanghai/Shenzhen correlation analysis
# Policy keyword detection and sentiment
# Overnight gap risk management
# Currency factor integration
```

### 5. Low Float Squeeze (`low-float-squeeze.yml`)
**The Rocket Hunter** - Hunts explosive low-float squeezes:

```yaml
# 20x volume nuclear detection
# Float analysis and short interest
# Social sentiment integration
# Intraday-only with profit laddering
```

## ⚙️ Quick Setup Guide

### 1. Test Individual Strategies

```bash
# Test biotech strategy
dotnet run --project src -- hunt --strategy config/strategies/biotech-catalyst-hunter.yml --demo

# Test China ADR strategy  
dotnet run --project src -- hunt --strategy config/strategies/china-adr-momentum.yml --demo

# Test low float squeeze
dotnet run --project src -- hunt --strategy config/strategies/low-float-squeeze.yml --demo
```

### 2. Run Master Strategy (All Symbols)

```bash
# Hunt all 24 symbols with adaptive rules
dotnet run --project src -- hunt --strategy config/strategies/master-momentum-hunter.yml --demo
```

### 3. Override Specific Symbols

```bash
# Test Tesla using momentum rules
dotnet run --project src -- hunt --symbol SLRX --strategy config/strategies/biotech-catalyst-hunter.yml --demo

# Test Chinese stock
dotnet run --project src -- hunt --symbol CHNR --strategy config/strategies/china-adr-momentum.yml --demo
```

## 🎛️ Key Parameters by Strategy Type

### Biotech Parameters
```yaml
risk_per_attempt_gbp: 40        # Ultra-conservative
stop_loss_percent: 12           # Wide for binary events
take_profit_percent: 35         # High reward targets
volume_multiplier: 10.0         # Need massive volume
```

### China ADR Parameters  
```yaml
risk_per_attempt_gbp: 75        # Moderate risk
stop_loss_percent: 6            # Normal stop
overnight_risk_limit: 0.5       # Half size overnight
correlation_required: true      # Must correlate with China ETFs
```

### Low Float Parameters
```yaml
risk_per_attempt_gbp: 30        # Very conservative
stop_loss_percent: 15           # Very wide
max_hold_minutes: 240           # Intraday only
volume_multiplier: 20.0         # Nuclear volume needed
```

## 🔍 Entry Signal Checklist

### ✅ Universal Requirements (All Strategies)
- [ ] Volume >4x average (minimum)
- [ ] Fresh news catalyst (<48 hours)
- [ ] Price momentum >3% 
- [ ] Market not in panic (VIX <40)
- [ ] Not first/last 30 minutes of trading

### ✅ Biotech Additional Requirements
- [ ] Volume >10x average
- [ ] FDA/clinical trial related news
- [ ] Pre-market activity confirmation
- [ ] Short interest data available

### ✅ China ADR Additional Requirements  
- [ ] Correlation with FXI/KWEB >0.3
- [ ] Policy/macro news confirmation
- [ ] Yuan exchange rate factor
- [ ] Shanghai/Shenzhen market status

### ✅ Low Float Additional Requirements
- [ ] Volume >20x average (nuclear)
- [ ] Float <50M shares
- [ ] >30% of float trading
- [ ] Social media buzz confirmation

## 📊 Risk Management Rules

### Position Sizing by Category
| Category | Max Position | Risk per Trade | Stop Loss |
|----------|--------------|----------------|-----------|
| Biotech | 1.5% | £40 | 12% |
| China ADR | 3% | £75 | 6% |
| Low Float | 1% | £30 | 15% |
| Master Strategy | 2% | £60 | 10% |

### Daily Limits
- **Maximum Daily Loss:** £200
- **Maximum Concurrent Positions:** 4
- **Maximum Single Category:** 40%
- **Maximum High-Risk Positions:** 2

### Time Limits
- **Biotech:** 72 hours maximum hold
- **China ADR:** 3 days maximum  
- **Low Float:** 4 hours maximum (intraday only)
- **All:** Exit before earnings unless specifically targeting

## 📈 Expected Performance Metrics

### Target Statistics
| Metric | Biotech | China ADR | Low Float | Master |
|--------|---------|-----------|-----------|---------|
| Win Rate | 35% | 45% | 40% | 42% |
| Avg Win | 25% | 12% | 35% | 18% |
| Avg Loss | 10% | 5% | 12% | 8% |
| Profit Factor | 2.2 | 2.4 | 2.8 | 2.3 |
| Max Drawdown | 20% | 15% | 25% | 18% |

### Key Success Factors
1. **News Quality** - Fresh, material catalysts
2. **Volume Confirmation** - Institutional participation  
3. **Technical Setup** - Clean breakouts
4. **Market Timing** - Favorable conditions
5. **Risk Management** - Strict position sizing

## ⚠️ Critical Warnings

### 🚨 High Risk Notice
These strategies target **speculative, volatile stocks** with:
- High failure rates (65% of trades may lose)
- Extreme volatility (30%+ moves common)
- News-dependent outcomes (binary events)
- Low liquidity risks (wide spreads)
- Halt/suspension risks

### 🛡️ Mandatory Safeguards
1. **Never exceed position limits**
2. **Always trade in demo mode first**
3. **Respect stop losses religiously**
4. **Don't trade earnings unless targeting**
5. **Monitor news constantly during trades**

### 🚫 When NOT to Trade
- VIX >40 (market panic)
- First/last 30 minutes of trading
- During FOMC meetings
- On low volume days (<1M shares)
- Without fresh news catalysts
- When already at loss limits

## 📚 Learning Resources

### Essential Reading
- `/docs/trading-insights/market-volatility-analysis.md`
- `/docs/trading-insights/correlation-regression-analysis.md`
- `/docs/adding-new-symbols.md`

### Live Practice
1. Start with paper trading
2. Use smallest position sizes
3. Focus on one category first
4. Track every trade outcome
5. Adjust based on performance

---

*"In momentum trading, the trend is your friend until the bend at the end. News creates the trend, volume confirms it, and discipline preserves your capital."*

**Trade safe. Trade smart. Hunt wisely.** 🎯