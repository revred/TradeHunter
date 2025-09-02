# TradeHunter

A configurable, rules-based trading strategy engine that can hunt for trades across any symbols using YAML-defined strategies.

## Features

- **Strategy Configuration**: Define trading strategies in YAML files with configurable entry/exit conditions
- **Multi-Symbol Support**: Hunt for trades across any financial symbols
- **CLI Interface**: Full command-line interface for running, listing, and validating strategies
- **MCP Service**: Model Context Protocol server for integration with AI assistants
- **Risk Management**: Configurable position sizing and risk controls per strategy
- **Demo Mode**: Safe simulation mode for testing strategies

## Quick Start

### Build and Run
```bash
dotnet build
dotnet run -- hunt --demo
```

### List Available Strategies
```bash
dotnet run -- list
```

### Run Specific Strategy
```bash
dotnet run -- hunt --strategy strategies/urg-energy.yml --symbol URG
```

### Validate Strategies
```bash
dotnet run -- validate
```

### Start MCP Server
```bash
dotnet run -- mcp --port 3000
```

## Strategy Configuration

Strategies are defined in YAML files in the `strategies/` directory. Each strategy defines:

- **Symbol**: Target trading symbol
- **Risk Management**: Budget, position sizing, stop losses
- **Entry Conditions**: Technical indicators, momentum, news sentiment
- **Exit Conditions**: Stop losses, take profits, time-based exits
- **Data Sources**: Price feeds, news feeds
- **Trading Configuration**: Demo/live mode, broker settings

### Example Strategy File

```yaml
name: "Generic Momentum Strategy"
description: "Configurable momentum-based trading strategy"
symbol: "AAPL"
enabled: true

risk_management:
  budget_gbp: 1000
  risk_per_attempt_gbp: 100
  max_position_size_percent: 5
  stop_loss_percent: 3
  take_profit_percent: 8

entry_conditions:
  - type: "price_momentum"
    parameters:
      timeframe: "5m"
      momentum_threshold: 0.015
      volume_increase: 2.0

exit_conditions:
  - type: "stop_loss"
    parameters:
      percent: 3
      trailing: true

trading:
  mode: "demo"
  broker: "simulator"
```

## CLI Commands

### Hunt for Trades
```bash
# Hunt using all enabled strategies
dotnet run -- hunt

# Hunt with specific strategy
dotnet run -- hunt --strategy strategies/my-strategy.yml

# Override symbol and risk settings
dotnet run -- hunt --symbol TSLA --risk-budget 2000 --risk-per-attempt 200

# Run in live mode (use with caution)
dotnet run -- hunt --demo false
```

### List Strategies
```bash
# List all strategies in default directory
dotnet run -- list

# List strategies in custom directory
dotnet run -- list --strategies-dir custom/strategies
```

### Validate Configuration
```bash
# Validate all strategies
dotnet run -- validate

# Validate specific strategy
dotnet run -- validate --strategy strategies/my-strategy.yml
```

### MCP Server Mode
```bash
# Start MCP server on default port 3000
dotnet run -- mcp

# Start on custom port
dotnet run -- mcp --port 8080
```

## MCP (Model Context Protocol) Integration

TradeHunter can run as an MCP server, providing AI assistants with trading strategy capabilities:

### Available Endpoints

- `GET /strategies` - List all available strategies
- `POST /hunt` - Start hunting for trades
- `POST /validate` - Validate strategy configurations
- `GET /health` - Health check

### Example MCP Usage

```bash
# Start the MCP server
dotnet run -- mcp --port 3000

# Query available strategies
curl http://localhost:3000/strategies

# Start hunting with specific parameters
curl -X POST http://localhost:3000/hunt \
  -H "Content-Type: application/json" \
  -d '{"symbol": "AAPL", "demoMode": true, "riskBudget": 1000}'
```

## Project Structure

```
TradeHunter/
├── TradeHunter.sln          # Solution file
├── TradeHunter.csproj       # Project file
├── strategies/              # Strategy configuration files
│   ├── urg-energy.yml       # URG-specific strategy
│   └── generic-momentum.yml # Generic momentum strategy
├── Program.cs               # Main entry point with CLI parsing
├── CliOptions.cs           # CLI command definitions
├── StrategyModels.cs       # YAML strategy model classes
├── StrategyLoader.cs       # Strategy loading logic
├── ConfigurableHunterEngine.cs # Multi-strategy execution engine
├── McpServer.cs            # MCP server implementation
├── Engine.cs               # Core trading engine
├── Models.cs               # Trading data models
├── Rules.cs                # Trading rule implementations
├── DataFeed.cs             # Market data feeds
├── NewsFeed.cs             # News feed implementations
├── Broker.cs               # Execution broker interfaces
├── Config.cs               # Configuration management
└── Utils.cs                # Utility functions
```

## Dependencies

- .NET 8.0
- YamlDotNet - YAML parsing
- CommandLineParser - CLI interface
- Microsoft.Extensions.Hosting - Application framework

## License

This project is for educational and research purposes. Use at your own risk.