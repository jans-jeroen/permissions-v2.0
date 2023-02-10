using Azure.Core;
using Azure.Identity;

public sealed class MicrosoftGraphAuthentication
{
    private String ClientID;
    private String TenantID;

    public MicrosoftGraphAuthentication(String clientID, String tenantID)
    {
        this.ClientID = clientID;
        this.TenantID = tenantID;
    }

    public TokenCredential GetCredentials()
    {
        // using Azure.Identity;
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        // Callback function that receives the user prompt
        // Prompt contains the generated device code that use must
        // enter during the auth process in the browser
        Func<DeviceCodeInfo, CancellationToken, Task> callback = (code, cancellation) =>
        {
            Console.WriteLine(code.Message);
            return Task.FromResult(0);
        };

        // https://learn.microsoft.com/dotnet/api/azure.identity.devicecodecredential
        return new DeviceCodeCredential(callback, this.ClientID, this.TenantID, options);
    }
}