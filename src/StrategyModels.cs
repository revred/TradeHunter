using YamlDotNet.Serialization;

namespace TradeHunter;

public class TradingStrategy
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    [YamlMember(Alias = "symbol")]
    public string Symbol { get; set; } = string.Empty;

    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;

    [YamlMember(Alias = "risk_management")]
    public RiskManagement RiskManagement { get; set; } = new();

    [YamlMember(Alias = "entry_conditions")]
    public List<TradingCondition> EntryConditions { get; set; } = new();

    [YamlMember(Alias = "exit_conditions")]
    public List<TradingCondition> ExitConditions { get; set; } = new();

    [YamlMember(Alias = "data_sources")]
    public DataSources DataSources { get; set; } = new();

    [YamlMember(Alias = "trading")]
    public TradingConfig Trading { get; set; } = new();
}

public class RiskManagement
{
    [YamlMember(Alias = "budget_gbp")]
    public decimal BudgetGbp { get; set; }

    [YamlMember(Alias = "risk_per_attempt_gbp")]
    public decimal RiskPerAttemptGbp { get; set; }

    [YamlMember(Alias = "max_position_size_percent")]
    public decimal MaxPositionSizePercent { get; set; }

    [YamlMember(Alias = "stop_loss_percent")]
    public decimal StopLossPercent { get; set; }

    [YamlMember(Alias = "take_profit_percent")]
    public decimal TakeProfitPercent { get; set; }
}

public class TradingCondition
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    [YamlMember(Alias = "parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class DataSources
{
    [YamlMember(Alias = "price_feed")]
    public string PriceFeed { get; set; } = string.Empty;

    [YamlMember(Alias = "news_feed")]
    public string NewsFeed { get; set; } = string.Empty;
}

public class TradingConfig
{
    [YamlMember(Alias = "mode")]
    public string Mode { get; set; } = "demo";

    [YamlMember(Alias = "broker")]
    public string Broker { get; set; } = "simulator";

    [YamlMember(Alias = "order_type")]
    public string OrderType { get; set; } = "market";

    [YamlMember(Alias = "limit_offset_percent")]
    public decimal? LimitOffsetPercent { get; set; }
}