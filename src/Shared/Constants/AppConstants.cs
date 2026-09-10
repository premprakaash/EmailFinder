namespace MailForge.Shared.Constants;

public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";
}

public static class QueueNames
{
    public const string Finder = "finder";
    public const string Verification = "verification";
    public const string Enrichment = "enrichment";
    public const string BulkImport = "bulk-import";
    public const string Export = "export";
    public const string Cleanup = "cleanup";
}

public static class ApiKeyPrefix
{
    public const string Prefix = "mf_";
}
