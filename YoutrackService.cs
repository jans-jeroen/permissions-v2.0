using System.Net.Http.Headers;
using System.Net.Http.Json;

public class YoutrackService : IDataService
{
    private String Server;
    private String Token;

    public YoutrackService(String server, String token)
    {
        this.Server = server;
        this.Token = token;
    }

    public void ProcessData(Dictionary<string, User> data)
    {
        HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri(this.Server),
        };

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, String.Format("{0}/hub/api/rest/users?fields=profile(email),transitiveProjectRoles(role(name),project(name))&query=not%20is:banned", this.Server));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.Token);

        HttpResponseMessage response = client.Send(request);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Invalid response");
        }

        UsersRecord? users = response.Content.ReadFromJsonAsync<UsersRecord>().GetAwaiter().GetResult();

        if (users == null) {
            throw new Exception("Missing youtrack users");
        }

        foreach (UserRecord user in users.Users)
        {
            String emailAddress = user.Profile.Email.Email;

            User userData = data.TryGetValue(emailAddress, out User u) ? u : new User();

            foreach (RolesRecord role in user.TransitiveProjectRoles)
            {
                HashSet<String> youtrackRoles = userData.Youtrack.TryGetValue(role.Project.Name, out HashSet<String> r) ? r : new HashSet<String>();
                youtrackRoles.Add(role.Role.Name);

                userData.Youtrack[role.Project.Name] = youtrackRoles;
            }

            data[emailAddress] = userData;
        }
    }

    record class UsersRecord(List<UserRecord> Users);

    record class UserRecord(ProfileRecord Profile, List<RolesRecord> TransitiveProjectRoles);

    record class ProfileRecord(EmailRecord Email);

    record class EmailRecord(String Email);

    record class RolesRecord(NameRecord Role, NameRecord Project);

    record class NameRecord(String Name);
}