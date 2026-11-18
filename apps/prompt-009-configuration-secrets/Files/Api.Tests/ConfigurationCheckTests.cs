using Xunit;
using Api;
public class ConfigurationCheckTests
{
    [Fact]
    public void MissingValueIsNotConfigured() => Assert.False(ConfigurationCheck.HasApiKey(null));
    [Fact]
    public void NonemptyValueIsConfigured() => Assert.True(ConfigurationCheck.HasApiKey("dummy-local-value"));
}
