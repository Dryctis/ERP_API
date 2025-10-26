namespace ERP_API.Common.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "SqlServer";
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}