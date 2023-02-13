using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;

public class WebstockService : IDataService
{
    private String ConnectionString;

    public WebstockService(String connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public void ProcessData(Dictionary<string, User> data)
    {
        SqlConnection connection = new SqlConnection(this.ConnectionString);
        connection.Open();

        SqlCommand query = new SqlCommand("SELECT users.emailaddress, usergroups.name, users.Username FROM users JOIN usergroups ON users.usergroup_id = usergroups.usergroup_id WHERE users.active = 1;", connection);
        SqlDataReader reader = query.ExecuteReader();

        while (reader.Read())
        {
            SqlString emailAddress = reader.GetSqlString(0);
            String profile = reader.GetString(1);
            String username = reader.GetString(2);

            if (emailAddress.IsNull)
            {
                Console.WriteLine("Missing e-mail address for user {0}", username);
                continue;
            }


            User userData = data.TryGetValue(emailAddress.Value, out User u) ? u : new User();

            userData.Webstock = profile;

            data[emailAddress.Value] = userData;
        }

        reader.Close();
        connection.Close();
    }
}
