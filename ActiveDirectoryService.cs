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

    public void ProcessData(UserData data)
    {
        String ldapSearchBaseDN = "ou=users,ou=reworksnl,dc=reworksnl,dc=local";
        String ldapSearchQuery = "(objectClass=organizationalPerson)";
        String[] ldapSearchAttributes = { "dn", "mail", "sAMAccountName", "givenName", "sn", "cn", "memberOf", "title", "userAccountControl" };

        LdapDirectoryIdentifier ldapServer = new LdapDirectoryIdentifier(this.server, this.Port);
        NetworkCredential ldapCredentials = new NetworkCredential(this.Username, this.Password);

        LdapConnection ldapConnection = new LdapConnection(ldapServer, ldapCredentials, AuthType.Basic);
        ldapConnection.Bind();

        SearchRequest request = new SearchRequest(ldapSearchBaseDN, ldapSearchQuery, SearchScope.Subtree, ldapSearchAttributes);

        System.DirectoryServices.Protocols.SearchResponse response = (System.DirectoryServices.Protocols.SearchResponse)ldapConnection.SendRequest(request);

        Type stringType = typeof(String);

        foreach (SearchResultEntry entry in response.Entries)
        {
            int flags = GetAttributeIntValue(entry, "userAccountControl");
            String username = GetAttributeValue(entry, "sAMAccountName");
            String firstName = GetAttributeValue(entry, "givenName");
            String lastName = GetAttributeValue(entry, "sn");
            String firstNameUnderscoreLastName = firstName + "_" + lastName;
            String emailAddress = GetAttributeValue(entry, "mail");
            String title = GetAttributeValue(entry, "title");
            String? functionGroup = GetFunctionGroup(entry);

            // Skip users without an e-mail address (TODO: Log)
            if (emailAddress == "")
            {
                continue;
            }

            if (Convert.ToBoolean(flags & 0x0002)) {
                Console.WriteLine("Skipping user {0} as it is inactive", username);
                continue;
            }

            User user = data.GetOrCreate(emailAddress);
            user.Title = title;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.WindowsUsername = username;
            user.FunctionGroup = functionGroup;
            user.WindowsDistinguishedName = entry.DistinguishedName;

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

    private int GetAttributeIntValue(SearchResultEntry entry, String attribute)
    {
        string value = GetAttributeValue(entry, attribute);
        if (value == "") {
            return 0;
        }

        return int.Parse(value);
    }

    private String? GetFunctionGroup(SearchResultEntry entry)
    {
        String attribute = "memberOf";

        String? functionGroup = null;
        if (entry.Attributes.Contains(attribute))
        {
            for (int i = 0; i < entry.Attributes[attribute].Count; i++)
            {
                String? groupName = entry.Attributes[attribute][i].ToString();
                if (groupName == null || !groupName.StartsWith("CN=FG -"))
                {
                    continue;
                }

                if (functionGroup != null)
                {
                    throw new Exception("The entry has multiple function groups");
                }

                // This Substring isn't 100% correct but it's the fastest solution for now
                functionGroup = groupName.Substring(3, groupName.IndexOf(',') - 3);
            }
        }

        return functionGroup;
    }
}
