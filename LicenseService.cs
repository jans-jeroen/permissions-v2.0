using Azure.Core;
using Microsoft.Graph;

class LicenseService : IDataService
{
    // Multi-tenant apps can use "common",
    // single-tenant apps must use the tenant ID from the Azure portal
    private TokenCredential Credentials;

    public LicenseService(TokenCredential credentials)
    {
        this.Credentials = credentials;
    }

    public void ProcessData(Dictionary<string, User> data)
    {
        var scopes = new[] { "Organization.Read.All", "User.Read.All" };

        var graphClient = new GraphServiceClient(this.Credentials, scopes);

        var licenseMapping = new Dictionary<Guid, SubscribedSku>();

        var licenses = graphClient.SubscribedSkus.Request().GetAsync().GetAwaiter().GetResult();
        foreach (var license in licenses)
        {
            if (license.SkuId == null)
            {
                continue;
            }

            //Console.WriteLine("Total license count for SKU {0}: {1}/{2}", license.SkuPartNumber, license.ConsumedUnits, license.PrepaidUnits.Enabled);

            licenseMapping.Add((Guid)license.SkuId, license);
        }

        var users = graphClient.Users.Request().Select("id,mail,assignedLicenses").GetAsync().GetAwaiter().GetResult();
        foreach (var user in users)
        {
            User userData = data.TryGetValue(user.Mail, out User u) ? u : new User();
            
            foreach (var license in user.AssignedLicenses)
            {
                userData.Licenses.Add(licenseMapping[(Guid)license.SkuId].SkuPartNumber);
            }

            data[user.Mail] = userData;
        }
    }
}
