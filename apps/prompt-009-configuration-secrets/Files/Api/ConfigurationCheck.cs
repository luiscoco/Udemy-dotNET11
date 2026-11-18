namespace Api;
public static class ConfigurationCheck
{
    public static bool HasApiKey(string? value) => !string.IsNullOrWhiteSpace(value);
}
