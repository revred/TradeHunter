# 📈 Adding New Symbols to TradeHunter

This guide provides comprehensive instructions on how to add new trading symbols (stocks, ETFs, cryptocurrencies, etc.) to the TradeHunter platform. Whether you're adding a single stock or an entire portfolio, this guide has you covered.

## Table of Contents
- [Quick Start](#quick-start)
- [Strategy Configuration Methods](#strategy-configuration-methods)
- [Creating Custom Strategies](#creating-custom-strategies)
- [Symbol-Specific Optimizations](#symbol-specific-optimizations)
- [Testing New Symbols](#testing-new-symbols)
- [Best Practices](#best-practices)

---

## 🚀 Quick Start

### Method 1: Command Line Override (Fastest)
Override any strategy's symbol directly from the command line:

```bash
# Hunt TSLA using existing momentum strategy
dotnet run --project src -- hunt --symbol TSLA --demo --strategies-dir config/strategies

# Hunt multiple symbols by running multiple instances
dotnet run --project src -- hunt --symbol AAPL --demo &
dotnet run --project src -- hunt --symbol GOOGL --demo &
dotnet run --project src -- hunt --symbol MSFT --demo &
```

### Method 2: Copy & Modify Existing Strategy
1. Copy an existing strategy file:
```bash
cp config/strategies/generic-momentum.yml config/strategies/my-tesla-strategy.yml
```

2. Edit the new file and change the symbol:
```yaml
name: "Tesla Momentum Strategy"
description: "High volatility momentum strategy for TSLA"
symbol: "TSLA"  # ← Change this to your desired symbol
enabled: true
# ... rest of configuration
```

3. Run with your new strategy:
```bash
dotnet run --project src -- hunt --strategy config/strategies/my-tesla-strategy.yml --demo
```

---

## 📋 Strategy Configuration Methods

### Creating a New Strategy File

Create a new YAML file in `config/strategies/` with your symbol configuration:

**Example: `config/strategies/nvidia-ai-surge.yml`**

```yaml
# Strategy metadata
name: "NVIDIA AI Surge Strategy"
description: "Captures NVIDIA momentum during AI news cycles"
symbol: "NVDA"  # ← Your target symbol here
enabled: true

# Risk management specific to NVDA's volatility
risk_management:
  budget_gbp: 2000              # Higher budget for expensive stock
  risk_per_attempt_gbp: 200     # 10% risk per trade
  max_position_size_percent: 5   # Conservative due to high volatility
  stop_loss_percent: 4           # Wider stop for volatile stock
  take_profit_percent: 12        # Higher target for momentum

# Entry conditions optimized for NVDA
entry_conditions:
  - type: "price_momentum"
    parameters:
      timeframe: "15m"           # Shorter timeframe for day trading
      momentum_threshold: 0.025  # 2.5% move threshold
      volume_increase: 3.0       # High volume confirmation
      min_price: 400.0           # NVDA-specific price filter
  
  - type: "technical_indicator"
    parameters:
      indicator: "rsi"
      value: 35                  # Buy oversold conditions
      condition: "below"
  
  - type: "news_sentiment"
    parameters:
      keywords: ["AI", "chips", "datacenter", "GPU", "Jensen Huang"]
      sentiment_threshold: 0.8
      news_age_hours: 12

# Exit conditions
exit_conditions:
  - type: "stop_loss"
    parameters:
      percent: 4
      trailing: true             # Trail stop as price rises
      
  - type: "take_profit"
    parameters:
      percent: 12
      
  - type: "technical_indicator"
    parameters:
      indicator: "rsi"
      value: 70
      condition: "above"         # Exit on overbought

# Data configuration
data_sources:
  price_feed: "yahoo_finance"
  news_feed: "financial_news_api"
  
trading:
  mode: "demo"
  broker: "simulator"
  order_type: "limit"
  limit_offset_percent: 0.1
```

### Multi-Symbol Strategy (Portfolio Approach)

Create a strategy that rotates between multiple symbols:

**Example: `config/strategies/tech-giants-rotation.yml`**

```yaml
name: "Tech Giants Rotation"
description: "Rotates between FAANG stocks based on momentum"
symbol: "AAPL"  # Primary symbol
enabled: true

# Additional symbols for rotation (custom extension)
additional_symbols:
  - "GOOGL"
  - "AMZN"
  - "META"
  - "NFLX"
  - "MSFT"

risk_management:
  budget_gbp: 5000
  risk_per_attempt_gbp: 500
  max_position_size_percent: 20  # Larger positions for blue chips
  stop_loss_percent: 2
  take_profit_percent: 6

# Rotation logic in entry conditions
entry_conditions:
  - type: "relative_strength"
    parameters:
      compare_to: "SPY"
      min_outperformance: 0.02   # Must outperform SPY by 2%
      lookback_days: 5
      
  - type: "sector_momentum"
    parameters:
      sector: "XLK"               # Technology sector ETF
      correlation_threshold: 0.7
      
  - type: "volume_surge"
    parameters:
      surge_multiplier: 2.5
      compare_to_average: 20      # 20-day average volume
```

---

## 🛠️ Creating Custom Strategies

### Step-by-Step Guide

#### 1. Analyze Your Symbol First

Before creating a strategy, understand your symbol's characteristics:

```python
# Use the provided market analysis tool
python tools/data-analysis/market-analysis.py

# In the script, add your symbol:
symbols = ['YOUR_SYMBOL', 'SPY']  # Compare to market
analyzer = MarketAnalyzer(symbols)
analyzer.export_analysis_report()
```

#### 2. Determine Optimal Parameters

Based on the analysis, choose appropriate parameters:

| Symbol Characteristic | Recommended Settings |
|----------------------|---------------------|
| **High Volatility** (>30% annual) | Wider stops (5-8%), smaller positions, shorter timeframes |
| **Low Volatility** (<15% annual) | Tighter stops (2-3%), larger positions, longer timeframes |
| **High Beta** (>1.5) | Market timing crucial, reduce size in volatile markets |
| **Low Beta** (<0.7) | Mean reversion strategies, increase position size |
| **High Volume** | Tighter spreads, market orders OK |
| **Low Volume** | Wider spreads, use limit orders only |

#### 3. Create Your Strategy File

```yaml
name: "Your Strategy Name"
description: "Clear description of strategy logic"
symbol: "YOUR_SYMBOL"
enabled: true

# Customize based on analysis
risk_management:
  budget_gbp: <based_on_account_size>
  risk_per_attempt_gbp: <1-2%_of_budget>
  max_position_size_percent: <based_on_volatility>
  stop_loss_percent: <based_on_ATR>
  take_profit_percent: <risk_reward_ratio>

# Add conditions specific to your symbol
entry_conditions:
  # ... your conditions

exit_conditions:
  # ... your conditions
```

#### 4. Backtest Your Strategy

```bash
# Test in demo mode first
dotnet run --project src -- hunt --strategy config/strategies/your-strategy.yml --demo

# Monitor the output for entry/exit signals
```

---

## 🎯 Symbol-Specific Optimizations

### Cryptocurrency Symbols

For crypto trading (BTC, ETH, etc.):

```yaml
name: "Bitcoin Volatility Hunter"
symbol: "BTC-USD"  # Use correct Yahoo Finance format
enabled: true

entry_conditions:
  - type: "price_momentum"
    parameters:
      timeframe: "1h"
      momentum_threshold: 0.03    # 3% moves common in crypto
      volume_increase: 2.0
      
  - type: "volatility_breakout"
    parameters:
      bollinger_bands_period: 20
      standard_deviations: 2
      
  - type: "time_filter"
    parameters:
      avoid_hours: ["02:00-06:00"]  # Low liquidity hours
      preferred_days: ["Mon", "Tue", "Wed", "Thu", "Fri"]

trading:
  mode: "demo"
  broker: "simulator"
  order_type: "limit"              # Always use limits in crypto
  limit_offset_percent: 0.2        # Wider spreads in crypto
```

### ETF Symbols

For ETF trading (SPY, QQQ, etc.):

```yaml
name: "SPY Mean Reversion"
symbol: "SPY"
enabled: true

entry_conditions:
  - type: "mean_reversion"
    parameters:
      bollinger_period: 20
      entry_std_dev: -2.0          # Buy at lower band
      
  - type: "vix_filter"
    parameters:
      max_vix: 30                  # Avoid high volatility
      min_vix: 12                  # Avoid complacency
      
exit_conditions:
  - type: "mean_reversion_exit"
    parameters:
      target: "middle_band"        # Exit at moving average
```

### Penny Stocks

For low-priced, high-volatility stocks:

```yaml
name: "Penny Stock Breakout"
symbol: "SNDL"  # Example penny stock
enabled: true

risk_management:
  budget_gbp: 500                  # Smaller budget
  risk_per_attempt_gbp: 50         # Limit risk
  max_position_size_percent: 10
  stop_loss_percent: 10            # Wider stops needed
  take_profit_percent: 25          # Higher targets

entry_conditions:
  - type: "volume_explosion"
    parameters:
      min_volume: 10000000         # Need high volume
      volume_multiplier: 5.0       # 5x average volume
      
  - type: "price_filter"
    parameters:
      min_price: 0.10             # Avoid trip-zero stocks
      max_price: 5.00             # Penny stock range
```

---

## 🧪 Testing New Symbols

### 1. Validate Strategy Configuration

```bash
# Validate your new strategy file
dotnet run --project src -- validate --strategy config/strategies/your-new-strategy.yml
```

### 2. Run in Demo Mode

```bash
# Always test in demo mode first
dotnet run --project src -- hunt --strategy config/strategies/your-new-strategy.yml --demo

# With extra logging for debugging
dotnet run --project src -- hunt --strategy config/strategies/your-new-strategy.yml --demo --verbose
```

### 3. Paper Trade

Before going live, paper trade for at least 2 weeks:

```yaml
# In your strategy file
trading:
  mode: "demo"        # Keep in demo
  paper_trading: true # Track as if real
```

### 4. Monitor Performance

Track key metrics:
- Win rate
- Average win/loss
- Maximum drawdown
- Sharpe ratio
- Number of trades

---

## ✅ Best Practices

### 1. Symbol Naming Conventions

Use correct symbol formats for different exchanges:

| Market | Format | Example |
|--------|--------|---------|
| US Stocks | TICKER | AAPL, MSFT, TSLA |
| Forex | BASE-QUOTE | EUR-USD, GBP-USD |
| Crypto | COIN-USD | BTC-USD, ETH-USD |
| International | TICKER.EXCHANGE | HSBA.L (London) |

### 2. Risk Management by Symbol Type

```yaml
# Blue Chip Stocks (AAPL, MSFT)
risk_management:
  risk_per_attempt_gbp: 100-200  # Can risk more
  stop_loss_percent: 2-3          # Tighter stops

# Volatile Growth (TSLA, NVDA)
risk_management:
  risk_per_attempt_gbp: 50-100   # Risk less
  stop_loss_percent: 5-8          # Wider stops

# Penny Stocks
risk_management:
  risk_per_attempt_gbp: 25-50    # Minimal risk
  stop_loss_percent: 10-15       # Very wide stops
```

### 3. Multiple Strategies Per Symbol

Create different strategies for different market conditions:

```bash
config/strategies/
├── tesla-momentum.yml       # For trending markets
├── tesla-mean-reversion.yml  # For range-bound markets
├── tesla-earnings.yml        # For earnings plays
└── tesla-options-hedge.yml   # For hedged positions
```

### 4. Symbol Groups

Organize related symbols:

```yaml
# config/strategies/semiconductor-sector.yml
symbols_group:
  leaders: ["NVDA", "AMD", "INTC"]
  emerging: ["MRVL", "QCOM", "MU"]
  etf: ["SMH", "SOXX"]
```

### 5. Dynamic Symbol Selection

Use screeners to find symbols dynamically:

```yaml
entry_conditions:
  - type: "screener"
    parameters:
      market_cap_min: 1000000000  # $1B minimum
      average_volume_min: 1000000 # 1M shares daily
      price_range: [10, 500]       # Price between $10-500
      sector: "Technology"
```

---

## 📚 Advanced Symbol Configuration

### Custom Data Sources

For symbols needing special data:

```yaml
data_sources:
  price_feed: "interactive_brokers"  # For better data
  news_feed: "benzinga"               # For penny stock news
  fundamental_data: "alpha_vantage"   # For earnings data
  options_data: "tradier"             # For options flow
```

### Symbol-Specific Indicators

Add custom indicators for specific symbols:

```yaml
custom_indicators:
  - name: "uranium_spot_price"
    source: "custom_api"
    weight: 0.3
    
  - name: "tesla_delivery_numbers"
    source: "quarterly_reports"
    weight: 0.5
```

---

## 🎓 Examples Repository

Find pre-configured strategies for popular symbols in:

```
config/strategies/examples/
├── stocks/
│   ├── aapl-swing-trade.yml
│   ├── tsla-day-trade.yml
│   └── nvda-momentum.yml
├── etfs/
│   ├── spy-mean-reversion.yml
│   └── qqq-trend-following.yml
├── crypto/
│   ├── btc-breakout.yml
│   └── eth-accumulation.yml
└── sectors/
    ├── tech-rotation.yml
    └── energy-momentum.yml
```

---

## 🆘 Troubleshooting

### Symbol Not Found
```bash
# Check symbol format
dotnet run --project src -- validate --symbol YOUR_SYMBOL

# Verify data source supports symbol
dotnet run --project src -- test-data --symbol YOUR_SYMBOL
```

### No Trading Signals
- Check market hours for the symbol
- Verify entry conditions aren't too restrictive
- Ensure data feed is working
- Check if symbol is halted/delisted

### Poor Performance
- Review market analysis for symbol characteristics
- Adjust position sizing based on volatility
- Optimize entry/exit conditions
- Consider different timeframes

---

## 📞 Getting Help

- **Documentation**: `/docs/trading-insights/`
- **Examples**: `/config/strategies/examples/`
- **Issues**: [GitHub Issues](https://github.com/revred/TradeHunter/issues)
- **Community**: [Discussions](https://github.com/revred/TradeHunter/discussions)

---

*Remember: Always test new symbols in demo mode before risking real capital. The market can remain irrational longer than you can remain solvent.*