# Correlation & Regression Analysis for Systematic Trading

## Introduction

Understanding how stocks move in relation to each other, market indices, and macroeconomic factors is crucial for building robust trading strategies. This document explores correlation analysis, regression models, and their practical applications in systematic trading through the TradeHunter framework.

## Correlation Fundamentals

### Pearson Correlation Coefficient
The standard measure of linear relationship between two variables:

```
ρ(X,Y) = Cov(X,Y) / (σ_X × σ_Y)

Where:
ρ ∈ [-1, 1]
ρ = 1: Perfect positive correlation
ρ = 0: No linear correlation  
ρ = -1: Perfect negative correlation
```

### Dynamic Correlation
Correlations change over time, especially during market stress:

```
Rolling_Correlation(t) = Corr(X[t-n:t], Y[t-n:t])

Typical lookback periods:
- Short-term: 20 days
- Medium-term: 60 days  
- Long-term: 252 days
```

## Market Index Correlations

### SPY Correlation Analysis

The S&P 500 (SPY) serves as the primary market benchmark. Stock correlations with SPY reveal important characteristics:

#### High SPY Correlation (ρ > 0.8)
**Characteristics:**
- Market-sensitive stocks
- Cyclical sectors (Technology, Financials)
- Large-cap growth stocks
- High beta names

**Examples:** AAPL, MSFT, GOOGL, JPM

**Trading Implications:**
- Market timing becomes crucial
- Sector rotation strategies effective
- Index hedging opportunities
- Momentum strategies in trending markets

#### Medium SPY Correlation (0.3 < ρ < 0.8)
**Characteristics:**
- Semi-independent price action
- Sector-specific drivers
- Mid-cap stocks
- Defensive sectors with growth components

**Examples:** Healthcare, Consumer Discretionary

**Trading Implications:**
- Stock-specific analysis important
- Earnings and company news drive moves
- Pairs trading opportunities
- Diversification benefits

#### Low SPY Correlation (ρ < 0.3)
**Characteristics:**
- Independent price drivers
- Defensive sectors
- Utilities, REITs
- Commodity-exposed stocks

**Examples:** Utilities (XLU), Gold miners (GLD), REITs (VNQ)

**Trading Implications:**
- Portfolio diversification value
- Hedge against market downturns
- Specific fundamental analysis required
- Mean reversion strategies often effective

### Sector Correlation Patterns

#### Technology Sector (XLK)
```
Average Intra-Sector Correlation: 0.65
Peak Correlation (Crisis): 0.85+
Correlation with SPY: 0.92

Key Relationships:
- AAPL ↔ MSFT: 0.72
- NVDA ↔ AMD: 0.78
- Cloud stocks (CRM, NOW, ADBE): 0.68
```

**Trading Strategy Implications:**
- Sector momentum strategies highly effective
- Single stock risk is amplified by sector moves
- Earnings season creates correlation spikes
- Innovation cycles drive collective moves

#### Financial Sector (XLF)
```
Average Intra-Sector Correlation: 0.58
Interest Rate Sensitivity: High
Correlation with 10Y Treasury: -0.45

Key Relationships:
- Big Banks (JPM, BAC, WFC): 0.72
- Regional Banks correlation: 0.65
- Insurance correlation: 0.52
```

**Trading Strategy Implications:**
- Interest rate plays crucial for timing
- Regulatory news affects entire sector
- Credit cycle positioning important
- Yield curve strategies applicable

### Cross-Sector Correlations

#### Technology vs Utilities
```
Normal Market: ρ ≈ 0.15
Crisis Market: ρ ≈ -0.25
Interest Rate Rising: ρ ≈ -0.35
```

This negative correlation creates natural hedging opportunities.

#### Energy vs Technology
```
Oil Bull Market: ρ ≈ -0.20
Oil Bear Market: ρ ≈ 0.10
Inflation Concerns: ρ ≈ -0.30
```

Energy and tech often move inversely during commodity cycles.

## Beta Analysis and Regression

### Single-Factor Model (CAPM)
```
R_stock = α + β × R_market + ε

Where:
β = Cov(R_stock, R_market) / Var(R_market)
α = Stock's excess return (alpha)
ε = Idiosyncratic risk
```

### Multi-Factor Models

#### Fama-French Three-Factor Model
```
R_stock = α + β₁×MKT + β₂×SMB + β₃×HML + ε

Where:
MKT = Market risk premium
SMB = Small minus Big (size factor)
HML = High minus Low (value factor)
```

#### Custom Factor Models for Trading
```yaml
custom_factors:
  market_factor: "SPY"
  sector_factor: "XLK"  # For tech stocks
  volatility_factor: "VIX"
  interest_rate_factor: "TLT"
  dollar_factor: "UUP"
```

### Beta Stability and Regime Changes

Beta is not constant - it changes based on market conditions:

#### Bull Market Beta
```
Typical Beta Range: 0.8 - 1.5
Correlation Stability: High
Mean Reversion: Strong
```

#### Bear Market Beta
```
Typical Beta Range: 1.2 - 2.0+
Correlation Stability: Low
Downside Sensitivity: Amplified
```

#### Crisis Beta
During market crises, correlations spike toward 1.0:
```
Normal Period: Average ρ = 0.45
Crisis Period: Average ρ = 0.75+
```

This "correlation breakdown" affects diversification benefits.

## Practical Trading Applications

### Pairs Trading

Identify stocks with high correlation but temporary divergence:

```yaml
pairs_trading_strategy:
  entry_conditions:
    - historical_correlation: ">0.7"
    - current_spread: ">2_standard_deviations"
    - cointegration_test: "passed"
  
  position_structure:
    - long_underperformer: true
    - short_outperformer: true
    - dollar_neutral: true
```

**Classic Pairs:**
- MSFT vs AAPL (ρ = 0.72)
- JPM vs BAC (ρ = 0.76)
- HD vs LOW (ρ = 0.68)
- PEP vs KO (ρ = 0.63)

### Market Neutral Strategies

Use correlation analysis to build market-neutral portfolios:

```
Portfolio_Beta = Σ(w_i × β_i) ≈ 0

Where:
w_i = Weight of stock i
β_i = Beta of stock i
```

### Sector Rotation

Exploit correlation patterns between sectors:

```yaml
sector_rotation:
  regime_detection:
    - rising_rates: "favor_financials_over_utilities"
    - falling_rates: "favor_utilities_over_financials"
    - growth_mode: "favor_tech_over_value"
    - inflation_mode: "favor_energy_over_tech"
```

### Risk Management

#### Correlation-Based Position Sizing
```
Adjusted_Position_Size = Base_Size / (1 + Correlation_Factor)

Where:
Correlation_Factor = Avg_Correlation × Number_of_Positions
```

#### Diversification Scoring
```
Diversification_Score = 1 - (Σ|ρ_ij|) / N²

Higher score = Better diversification
Target Score > 0.7 for uncorrelated strategies
```

## Advanced Correlation Techniques

### Rolling Correlation Analysis

Track how correlations change over time:

```python
def rolling_correlation_signal(stock_a, stock_b, window=60):
    """
    Generate trading signals based on correlation regime changes
    """
    correlation = stock_a.rolling(window).corr(stock_b)
    
    # Signal when correlation breaks down
    if correlation[-1] < correlation.quantile(0.2):
        return "PAIRS_TRADE_OPPORTUNITY"
    
    # Signal when correlation spikes (crisis)
    if correlation[-1] > correlation.quantile(0.9):
        return "REDUCE_LEVERAGE"
    
    return "NORMAL"
```

### Correlation Clustering

Groups of stocks that move together:

#### Technology Cluster
```
Core Stocks: AAPL, MSFT, GOOGL, AMZN
Satellite Stocks: NVDA, CRM, ADBE, NFLX
Average Intra-Cluster ρ: 0.68
```

#### Defensive Cluster
```
Core Stocks: JNJ, PG, KO, WMT
Satellite Stocks: MCD, PEP, CL, GIS
Average Intra-Cluster ρ: 0.42
```

### Lead-Lag Relationships

Some stocks lead others in the same sector:

```
AAPL leads AAPL suppliers (correlation with 1-day lag)
XLF leads regional banks (correlation with 2-day lag)
Oil prices lead energy stocks (correlation same day)
```

**Trading Application:**
```yaml
lead_lag_strategy:
  leader: "AAPL"
  followers: ["AAPL_SUPPLIERS"]
  signal_lag: 1
  entry_threshold: "2_percent_move"
```

## Volatility Correlation

Volatilities of related stocks are often correlated:

### VIX Relationships
```
High VIX Periods (>25):
- Stock correlations → 1.0
- Beta expansion
- Diversification breakdown

Low VIX Periods (<15):
- Stock correlations → normal
- Idiosyncratic risk dominates
- Better diversification
```

### Sector Volatility Clustering
```
Tech Sector Volatility Correlation: 0.75
Financial Sector Volatility Correlation: 0.68
Utility Sector Volatility Correlation: 0.45
```

## Macroeconomic Factor Analysis

### Interest Rate Sensitivity
```
Duration-Like Beta = % Stock Change / % Rate Change

High Sensitivity (β > 1.5):
- REITs, Utilities, High-dividend stocks
- Growth stocks with distant cash flows

Low Sensitivity (β < 0.5):
- Banks (benefit from rising rates)
- Commodity stocks
- Short-duration cash flow stocks
```

### Dollar Correlation
```
Strong Dollar Environment:
- Exporters: Negative correlation
- Importers: Positive correlation
- Multinationals: Negative correlation
- Domestic players: Low correlation
```

### Commodity Correlations
```
Energy Stocks vs Oil: ρ = 0.85
Mining Stocks vs Metals: ρ = 0.78
Agriculture vs Commodity Prices: ρ = 0.65
```

## Implementation in TradeHunter

### Strategy Configuration
```yaml
correlation_filters:
  market_correlation:
    symbol: "SPY"
    min_correlation: 0.3
    max_correlation: 0.9
    lookback_days: 60
    
  sector_correlation:
    sector_etf: "auto_detect"
    relative_strength_filter: true
    
  volatility_correlation:
    vix_regime_filter: true
    correlation_threshold: 0.7
```

### Risk Management Rules
```yaml
risk_management:
  correlation_limits:
    max_single_sector: 0.25
    max_high_beta: 0.30
    max_correlation_cluster: 0.20
    
  dynamic_sizing:
    correlation_adjustment: true
    crisis_detection: "vix_spike"
    leverage_reduction: 0.5
```

### Backtesting Framework
```yaml
backtest_settings:
  correlation_analysis: true
  rolling_windows: [20, 60, 252]
  regime_detection: true
  factor_attribution: true
  drawdown_correlation: true
```

## Performance Monitoring

### Correlation-Adjusted Metrics

#### Sharpe Ratio with Correlation Penalty
```
Adjusted_Sharpe = Sharpe_Ratio × (1 - Average_Portfolio_Correlation)
```

#### Maximum Drawdown Attribution
```
Correlation_Contribution = Portfolio_Correlation × Market_Drawdown
Specific_Risk = Total_Drawdown - Correlation_Contribution
```

### Real-Time Monitoring
```yaml
alerts:
  correlation_spike:
    threshold: 0.8
    action: "reduce_leverage"
    
  correlation_breakdown:
    threshold: 0.2
    action: "pairs_opportunity"
    
  sector_concentration:
    threshold: 0.4
    action: "rebalance_warning"
```

## Conclusion

Correlation and regression analysis provide the foundation for:

1. **Portfolio Construction** - Building diversified, risk-balanced portfolios
2. **Risk Management** - Understanding concentration and systematic risks  
3. **Strategy Development** - Identifying mean reversion and momentum opportunities
4. **Market Timing** - Using correlation regimes to adjust positioning
5. **Performance Attribution** - Separating alpha from beta returns

The TradeHunter framework integrates these analytical capabilities to create adaptive, correlation-aware trading strategies that perform across different market regimes.

---

*"In the orchestra of markets, correlation is the conductor. Understanding the relationships between instruments allows you to predict the symphony."*