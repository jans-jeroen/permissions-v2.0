using System.DirectoryServices.Protocols;
using System.Net;

public class ActiveDirectoryService : IDataService
{
    private String Username;
    private String Password;
    private String server;
    private int Port;

    public ActiveDirectoryService(String username, String password, String server, int port)
    {
        this.Username = username;
        this.Password = password;
        this.server = server;
        this.Port = port;
    }

    public void ProcessData(Dictionary<String, User> data)
    {
        String ldapSearchBaseDN = "ou=users,ou=reworksnl,dc=reworksnl,dc=local";
        String ldapSearchQuery = "(objectClass=organizationalPerson)";
        String[] ldapSearchAttributes = { "dn", "mail", "sAMAccountName", "givenName", "sn", "cn", "memberOf", "title" };

        LdapDirectoryIdentifier ldapServer = new LdapDirectoryIdentifier(this.server, this.Port);
        NetworkCredential ldapCredentials = new NetworkCredential(this.Username, this.Password);

        LdapConnection ldapConnection = new LdapConnection(ldapServer, ldapCredentials, AuthType.Basic);
        ldapConnection.Bind();

        SearchRequest request = new SearchRequest(ldapSearchBaseDN, ldapSearchQuery, SearchScope.Subtree, ldapSearchAttributes);

        System.DirectoryServices.Protocols.SearchResponse response = (System.DirectoryServices.Protocols.SearchResponse)ldapConnection.SendRequest(request);

        Type stringType = typeof(String);

        foreach (SearchResultEntry entry in response.Entries)
        {
            String username = GetAttributeValue(entry, "sAMAccountName");
            String firstName = GetAttributeValue(entry, "givenName");
            String lastName = GetAttributeValue(entry, "sn");
            String firstNameUnderscoreLastName = firstName + "_" + lastName;
            String emailAddress = GetAttributeValue(entry, "mail");
            String title = GetAttributeValue(entry, "title");

            // Skip users without an e-mail address (TODO: Log)
            if (emailAddress == "")
            {
                continue;
            }

            User user = data.TryGetValue(emailAddress, out User u) ? u : new User();
            user.Title = title;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.WindowsUsername = username;

            data[emailAddress] = user;
        }
    }

    private String GetAttributeValue(SearchResultEntry entry, String attribute)
    {
        Type stringType = typeof(String);

        String value = "";
        if (entry.Attributes.Contains(attribute))
        {
            value = (string)entry.Attributes[attribute].GetValues(stringType).FirstOrDefault("");
        }

        return value;
    }
}
