using MySqlConnector;

public class AdminService : IDataService
{
    private String ConnectionString;

    public AdminService(String connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public void ProcessData(Dictionary<string, User> data)
    {
        MySqlConnection connection = new MySqlConnection(this.ConnectionString);
        connection.Open();

        MySqlCommand query = new MySqlCommand("SELECT au.email, p.name FROM admin_user au JOIN user_profile up ON up.userid = au.userid JOIN profile p ON p.id = up.profileid WHERE au.active = 1 AND au.is_external_party = 0 GROUP by au.userid, p.name ORDER BY email ASC;", connection);
        MySqlDataReader reader = query.ExecuteReader();

        while (reader.Read())
        {
            String emailAddress = reader.GetString("email");
            String profile = reader.GetString("name");

            User userData = data.TryGetValue(emailAddress, out User u) ? u : new User();

            userData.AdminProfiles.Add(profile);

            data[emailAddress] = userData;
        }

        reader.Close();
        connection.Close();
    }
}
