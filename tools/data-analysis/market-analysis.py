"""
Market Analysis Tools for TradeHunter
Provides advanced analytical capabilities for trading strategy development
"""

import pandas as pd
import numpy as np
import yfinance as yf
from scipy import stats
from typing import Dict, List, Tuple, Optional
import matplotlib.pyplot as plt
import seaborn as sns
from datetime import datetime, timedelta
import warnings
warnings.filterwarnings('ignore')

class MarketAnalyzer:
    """
    Comprehensive market analysis toolkit for systematic trading strategies
    """
    
    def __init__(self, symbols: List[str], benchmark: str = 'SPY', lookback_days: int = 252):
        """
        Initialize MarketAnalyzer with symbols and parameters
        
        Args:
            symbols: List of stock symbols to analyze
            benchmark: Benchmark symbol (default: SPY)
            lookback_days: Historical data lookback period
        """
        self.symbols = symbols
        self.benchmark = benchmark
        self.lookback_days = lookback_days
        self.data: Optional[pd.DataFrame] = None
        self.returns: Optional[pd.DataFrame] = None
        
    def fetch_data(self) -> pd.DataFrame:
        """Fetch historical price data for all symbols"""
        print(f"📊 Fetching data for {len(self.symbols)} symbols...")
        
        all_symbols = self.symbols + [self.benchmark] if self.benchmark not in self.symbols else self.symbols
        
        end_date = datetime.now()
        start_date = end_date - timedelta(days=self.lookback_days + 50)  # Extra buffer
        
        try:
            data = yf.download(all_symbols, start=start_date, end=end_date)['Adj Close']
            self.data = data.dropna()
            self.returns = self.data.pct_change().dropna()
            print(f"✅ Successfully fetched {len(self.data)} days of data")
            return self.data
        except Exception as e:
            print(f"❌ Error fetching data: {e}")
            raise
    
    def calculate_volatility_metrics(self) -> Dict[str, Dict[str, float]]:
        """Calculate comprehensive volatility metrics for all symbols"""
        if self.returns is None:
            self.fetch_data()
        
        print("📈 Calculating volatility metrics...")
        
        metrics = {}
        
        for symbol in self.symbols:
            if symbol not in self.returns.columns:
                continue
                
            returns = self.returns[symbol].dropna()
            
            # Basic volatility metrics
            daily_vol = returns.std()
            annualized_vol = daily_vol * np.sqrt(252)
            
            # Rolling volatility (different windows)
            vol_20d = returns.rolling(20).std().iloc[-1] * np.sqrt(252)
            vol_60d = returns.rolling(60).std().iloc[-1] * np.sqrt(252)
            
            # Volatility percentiles
            vol_percentile = stats.percentileofscore(returns.rolling(60).std().dropna(), daily_vol)
            
            # Downside deviation
            downside_returns = returns[returns < 0]
            downside_vol = downside_returns.std() * np.sqrt(252) if len(downside_returns) > 0 else 0
            
            # Volatility clustering (ARCH effect)
            squared_returns = returns ** 2
            arch_stat = self._ljung_box_test(squared_returns)
            
            metrics[symbol] = {
                'annualized_volatility': annualized_vol,
                'volatility_20d': vol_20d,
                'volatility_60d': vol_60d,
                'volatility_percentile': vol_percentile,
                'downside_volatility': downside_vol,
                'upside_downside_ratio': (annualized_vol / downside_vol) if downside_vol > 0 else np.inf,
                'volatility_clustering': arch_stat > 0.05  # p-value interpretation
            }
        
        return metrics
    
    def calculate_correlation_matrix(self) -> Tuple[pd.DataFrame, Dict[str, float]]:
        """Calculate correlation matrix and benchmark correlations"""
        if self.returns is None:
            self.fetch_data()
        
        print("🔗 Calculating correlation matrix...")
        
        # Full correlation matrix
        correlation_matrix = self.returns[self.symbols].corr()
        
        # Benchmark correlations
        benchmark_correlations = {}
        if self.benchmark in self.returns.columns:
            for symbol in self.symbols:
                if symbol in self.returns.columns and symbol != self.benchmark:
                    correlation = self.returns[symbol].corr(self.returns[self.benchmark])
                    benchmark_correlations[symbol] = correlation
        
        return correlation_matrix, benchmark_correlations
    
    def calculate_beta_metrics(self) -> Dict[str, Dict[str, float]]:
        """Calculate beta and related risk metrics"""
        if self.returns is None:
            self.fetch_data()
        
        if self.benchmark not in self.returns.columns:
            print(f"⚠️ Benchmark {self.benchmark} not available")
            return {}
        
        print("📊 Calculating beta metrics...")
        
        benchmark_returns = self.returns[self.benchmark]
        beta_metrics = {}
        
        for symbol in self.symbols:
            if symbol not in self.returns.columns or symbol == self.benchmark:
                continue
                
            stock_returns = self.returns[symbol]
            
            # Calculate beta
            covariance = np.cov(stock_returns, benchmark_returns)[0, 1]
            market_variance = np.var(benchmark_returns)
            beta = covariance / market_variance if market_variance != 0 else 0
            
            # Calculate alpha (Jensen's alpha)
            mean_stock_return = stock_returns.mean() * 252
            mean_market_return = benchmark_returns.mean() * 252
            alpha = mean_stock_return - beta * mean_market_return
            
            # R-squared
            correlation = stock_returns.corr(benchmark_returns)
            r_squared = correlation ** 2
            
            # Upside/downside beta
            up_market = benchmark_returns > 0
            down_market = benchmark_returns < 0
            
            upside_beta = 0
            downside_beta = 0
            
            if up_market.sum() > 10:  # Ensure enough observations
                upside_cov = np.cov(stock_returns[up_market], benchmark_returns[up_market])[0, 1]
                upside_var = np.var(benchmark_returns[up_market])
                upside_beta = upside_cov / upside_var if upside_var != 0 else 0
            
            if down_market.sum() > 10:
                downside_cov = np.cov(stock_returns[down_market], benchmark_returns[down_market])[0, 1]
                downside_var = np.var(benchmark_returns[down_market])
                downside_beta = downside_cov / downside_var if downside_var != 0 else 0
            
            beta_metrics[symbol] = {
                'beta': beta,
                'alpha_annualized': alpha,
                'r_squared': r_squared,
                'upside_beta': upside_beta,
                'downside_beta': downside_beta,
                'beta_asymmetry': upside_beta - downside_beta
            }
        
        return beta_metrics
    
    def analyze_volume_patterns(self) -> Dict[str, Dict[str, float]]:
        """Analyze volume patterns and relationships with price movements"""
        print("📊 Analyzing volume patterns...")
        
        volume_metrics = {}
        
        for symbol in self.symbols:
            try:
                # Fetch volume data separately
                ticker = yf.Ticker(symbol)
                hist = ticker.history(period=f"{self.lookback_days}d")
                
                if hist.empty or 'Volume' not in hist.columns:
                    continue
                
                volume = hist['Volume']
                close = hist['Close']
                returns = close.pct_change().dropna()
                
                # Volume statistics
                avg_volume = volume.mean()
                volume_std = volume.std()
                volume_cv = volume_std / avg_volume  # Coefficient of variation
                
                # Volume-price relationship
                volume_price_corr = volume.corr(close)
                volume_return_corr = volume[1:].corr(returns)
                
                # Volume surge analysis
                volume_zscore = (volume - volume.rolling(20).mean()) / volume.rolling(20).std()
                volume_surges = (volume_zscore > 2).sum()
                surge_frequency = volume_surges / len(volume)
                
                # On-balance volume trend
                obv = self._calculate_obv(close, volume)
                obv_trend = self._calculate_trend_strength(obv)
                
                volume_metrics[symbol] = {
                    'avg_volume': avg_volume,
                    'volume_volatility': volume_cv,
                    'volume_price_correlation': volume_price_corr,
                    'volume_return_correlation': volume_return_corr,
                    'volume_surge_frequency': surge_frequency,
                    'obv_trend_strength': obv_trend
                }
                
            except Exception as e:
                print(f"⚠️ Error analyzing volume for {symbol}: {e}")
                continue
        
        return volume_metrics
    
    def identify_regime_changes(self) -> Dict[str, List[datetime]]:
        """Identify volatility and correlation regime changes"""
        if self.returns is None:
            self.fetch_data()
        
        print("🔄 Identifying regime changes...")
        
        regime_changes = {}
        
        # Volatility regime changes
        for symbol in self.symbols:
            if symbol not in self.returns.columns:
                continue
                
            returns = self.returns[symbol]
            
            # Rolling volatility
            rolling_vol = returns.rolling(60).std()
            vol_median = rolling_vol.median()
            
            # Identify regime switches (simplified Markov switching model)
            high_vol_regime = rolling_vol > vol_median * 1.5
            regime_switches = []
            
            previous_regime = False
            for date, is_high_vol in high_vol_regime.items():
                if is_high_vol != previous_regime:
                    regime_switches.append(date)
                previous_regime = is_high_vol
            
            regime_changes[symbol] = regime_switches
        
        return regime_changes
    
    def generate_trading_insights(self) -> Dict[str, str]:
        """Generate actionable trading insights based on analysis"""
        print("💡 Generating trading insights...")
        
        insights = {}
        
        # Get all metrics
        vol_metrics = self.calculate_volatility_metrics()
        corr_matrix, benchmark_corrs = self.calculate_correlation_matrix()
        beta_metrics = self.calculate_beta_metrics()
        volume_metrics = self.analyze_volume_patterns()
        
        for symbol in self.symbols:
            insight_parts = []
            
            # Volatility insights
            if symbol in vol_metrics:
                vol = vol_metrics[symbol]
                
                if vol['volatility_percentile'] > 80:
                    insight_parts.append("HIGH VOLATILITY REGIME - Consider reduced position sizes")
                elif vol['volatility_percentile'] < 20:
                    insight_parts.append("LOW VOLATILITY REGIME - Potential for volatility expansion")
                
                if vol['upside_downside_ratio'] > 2:
                    insight_parts.append("ASYMMETRIC RISK - More upside than downside volatility")
            
            # Correlation insights
            if symbol in benchmark_corrs:
                corr = benchmark_corrs[symbol]
                
                if corr > 0.8:
                    insight_parts.append("HIGH MARKET CORRELATION - Market timing crucial")
                elif corr < 0.3:
                    insight_parts.append("LOW MARKET CORRELATION - Stock-specific factors dominate")
            
            # Beta insights
            if symbol in beta_metrics:
                beta = beta_metrics[symbol]
                
                if beta['beta'] > 1.5:
                    insight_parts.append("HIGH BETA - Amplified market movements")
                elif beta['beta'] < 0.5:
                    insight_parts.append("LOW BETA - Defensive characteristics")
                
                if beta['beta_asymmetry'] > 0.3:
                    insight_parts.append("UPSIDE LEVERAGE - Better performance in bull markets")
            
            # Volume insights
            if symbol in volume_metrics:
                vol_met = volume_metrics[symbol]
                
                if vol_met['volume_surge_frequency'] > 0.1:
                    insight_parts.append("FREQUENT VOLUME SPIKES - News-driven or institutional activity")
                
                if abs(vol_met['volume_return_correlation']) > 0.3:
                    insight_parts.append("VOLUME-PRICE RELATIONSHIP - Volume confirms price moves")
            
            insights[symbol] = " | ".join(insight_parts) if insight_parts else "NORMAL MARKET BEHAVIOR"
        
        return insights
    
    def create_strategy_recommendations(self) -> Dict[str, Dict[str, any]]:
        """Create specific strategy recommendations for each symbol"""
        print("🎯 Creating strategy recommendations...")
        
        recommendations = {}
        
        # Get metrics
        vol_metrics = self.calculate_volatility_metrics()
        beta_metrics = self.calculate_beta_metrics()
        corr_matrix, benchmark_corrs = self.calculate_correlation_matrix()
        
        for symbol in self.symbols:
            rec = {
                'primary_strategy': 'momentum',
                'position_sizing': 'normal',
                'risk_multiplier': 1.0,
                'stop_loss_adjustment': 1.0,
                'optimal_timeframes': ['1h', '4h', '1d'],
                'avoid_conditions': [],
                'special_considerations': []
            }
            
            # Volatility-based recommendations
            if symbol in vol_metrics:
                vol = vol_metrics[symbol]
                
                if vol['volatility_percentile'] > 75:
                    rec['position_sizing'] = 'reduced'
                    rec['risk_multiplier'] = 0.6
                    rec['stop_loss_adjustment'] = 1.5
                    rec['avoid_conditions'].append('earnings_week')
                
                if vol['volatility_clustering']:
                    rec['special_considerations'].append('volatility_clustering_detected')
                    rec['optimal_timeframes'] = ['5m', '15m', '1h']
            
            # Beta-based recommendations
            if symbol in beta_metrics:
                beta_data = beta_metrics[symbol]
                
                if beta_data['beta'] > 1.5:
                    rec['primary_strategy'] = 'momentum'
                    rec['special_considerations'].append('high_beta_amplified_moves')
                elif beta_data['beta'] < 0.7:
                    rec['primary_strategy'] = 'mean_reversion'
                    rec['special_considerations'].append('defensive_low_beta')
                
                if beta_data['beta_asymmetry'] > 0.2:
                    rec['special_considerations'].append('bull_market_outperformer')
            
            # Correlation-based recommendations
            if symbol in benchmark_corrs:
                corr = benchmark_corrs[symbol]
                
                if corr > 0.8:
                    rec['avoid_conditions'].extend(['market_uncertainty', 'vix_spike'])
                    rec['special_considerations'].append('market_timing_critical')
                elif corr < 0.3:
                    rec['special_considerations'].append('idiosyncratic_risk_dominant')
                    rec['position_sizing'] = 'increased'
                    rec['risk_multiplier'] = 1.3
            
            recommendations[symbol] = rec
        
        return recommendations
    
    def export_analysis_report(self, filename: str = None) -> str:
        """Export comprehensive analysis report"""
        if filename is None:
            filename = f"tradehunter_analysis_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"
        
        print(f"📝 Exporting analysis report to {filename}...")
        
        with open(filename, 'w') as f:
            f.write("=" * 80 + "\n")
            f.write("TRADEHUNTER MARKET ANALYSIS REPORT\n")
            f.write(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"Symbols Analyzed: {', '.join(self.symbols)}\n")
            f.write(f"Benchmark: {self.benchmark}\n")
            f.write(f"Analysis Period: {self.lookback_days} days\n")
            f.write("=" * 80 + "\n\n")
            
            # Volatility Analysis
            f.write("VOLATILITY ANALYSIS\n")
            f.write("-" * 40 + "\n")
            vol_metrics = self.calculate_volatility_metrics()
            for symbol, metrics in vol_metrics.items():
                f.write(f"\n{symbol}:\n")
                for metric, value in metrics.items():
                    f.write(f"  {metric}: {value:.4f}\n")
            
            # Beta Analysis  
            f.write("\n\nBETA ANALYSIS\n")
            f.write("-" * 40 + "\n")
            beta_metrics = self.calculate_beta_metrics()
            for symbol, metrics in beta_metrics.items():
                f.write(f"\n{symbol}:\n")
                for metric, value in metrics.items():
                    f.write(f"  {metric}: {value:.4f}\n")
            
            # Trading Insights
            f.write("\n\nTRADING INSIGHTS\n")
            f.write("-" * 40 + "\n")
            insights = self.generate_trading_insights()
            for symbol, insight in insights.items():
                f.write(f"\n{symbol}: {insight}\n")
            
            # Strategy Recommendations
            f.write("\n\nSTRATEGY RECOMMENDATIONS\n")
            f.write("-" * 40 + "\n")
            recommendations = self.create_strategy_recommendations()
            for symbol, rec in recommendations.items():
                f.write(f"\n{symbol}:\n")
                f.write(f"  Primary Strategy: {rec['primary_strategy']}\n")
                f.write(f"  Position Sizing: {rec['position_sizing']}\n")
                f.write(f"  Risk Multiplier: {rec['risk_multiplier']}\n")
                f.write(f"  Stop Loss Adjustment: {rec['stop_loss_adjustment']}\n")
                f.write(f"  Optimal Timeframes: {', '.join(rec['optimal_timeframes'])}\n")
                if rec['avoid_conditions']:
                    f.write(f"  Avoid Conditions: {', '.join(rec['avoid_conditions'])}\n")
                if rec['special_considerations']:
                    f.write(f"  Special Considerations: {', '.join(rec['special_considerations'])}\n")
        
        print(f"✅ Analysis report exported to {filename}")
        return filename
    
    # Helper methods
    def _ljung_box_test(self, series: pd.Series, lags: int = 10) -> float:
        """Ljung-Box test for autocorrelation (simplified)"""
        try:
            from statsmodels.stats.diagnostic import acorr_ljungbox
            result = acorr_ljungbox(series, lags=lags, return_df=True)
            return result['lb_pvalue'].iloc[-1]
        except ImportError:
            # Fallback if statsmodels not available
            return 0.5
    
    def _calculate_obv(self, close: pd.Series, volume: pd.Series) -> pd.Series:
        """Calculate On-Balance Volume"""
        obv = np.zeros(len(close))
        obv[0] = volume.iloc[0]
        
        for i in range(1, len(close)):
            if close.iloc[i] > close.iloc[i-1]:
                obv[i] = obv[i-1] + volume.iloc[i]
            elif close.iloc[i] < close.iloc[i-1]:
                obv[i] = obv[i-1] - volume.iloc[i]
            else:
                obv[i] = obv[i-1]
        
        return pd.Series(obv, index=close.index)
    
    def _calculate_trend_strength(self, series: pd.Series, window: int = 20) -> float:
        """Calculate trend strength using linear regression slope"""
        try:
            x = np.arange(len(series[-window:]))
            y = series[-window:].values
            slope, _, r_value, _, _ = stats.linregress(x, y)
            return slope * r_value  # Slope adjusted by R-squared
        except:
            return 0.0

# Example usage and main execution
if __name__ == "__main__":
    # Example symbols for analysis
    symbols = ['AAPL', 'MSFT', 'GOOGL', 'TSLA', 'NVDA', 'URG']
    
    # Initialize analyzer
    analyzer = MarketAnalyzer(symbols, benchmark='SPY', lookback_days=252)
    
    try:
        # Fetch data
        data = analyzer.fetch_data()
        
        # Run analysis
        print("\n🔬 Running comprehensive market analysis...")
        
        vol_metrics = analyzer.calculate_volatility_metrics()
        corr_matrix, benchmark_corrs = analyzer.calculate_correlation_matrix()
        beta_metrics = analyzer.calculate_beta_metrics()
        volume_metrics = analyzer.analyze_volume_patterns()
        insights = analyzer.generate_trading_insights()
        recommendations = analyzer.create_strategy_recommendations()
        
        # Export report
        report_file = analyzer.export_analysis_report()
        
        print(f"\n✅ Analysis complete! Report saved as: {report_file}")
        
        # Display key insights
        print("\n🎯 Key Trading Insights:")
        for symbol, insight in insights.items():
            print(f"{symbol}: {insight}")
            
    except Exception as e:
        print(f"❌ Analysis failed: {e}")