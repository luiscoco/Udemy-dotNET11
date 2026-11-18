using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
public interface ISecretSource {Task<string> Read(string name);}
public sealed class DevelopmentSecrets(IConfiguration configuration):ISecretSource
{
    public Task<string> Read(string name)=>Task.FromResult(configuration[name]
        ??throw new InvalidOperationException("Required development secret is missing"));
}
public sealed class VaultSecrets : ISecretSource
{
    private readonly SecretClient client;
    public VaultSecrets(Uri vault)
    {
        if(vault.Scheme!="https"||!vault.Host.EndsWith(".vault.azure.net",StringComparison.OrdinalIgnoreCase)||vault.UserInfo.Length!=0)
            throw new InvalidOperationException("An explicit HTTPS Key Vault endpoint is required");
        client=new SecretClient(vault,new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned));
    }
    public async Task<string> Read(string name)=>(await client.GetSecretAsync(name)).Value.Value;
}
public static class SecretSources
{
    public static ISecretSource Select(string environment,IConfiguration configuration)
    {
        if(environment=="Development")return new DevelopmentSecrets(configuration);
        var endpoint=configuration["Vault:Uri"]??throw new InvalidOperationException("Production vault is not configured");
        return new VaultSecrets(new Uri(endpoint)); // No fallback to a developer file or embedded credential.
    }
}
