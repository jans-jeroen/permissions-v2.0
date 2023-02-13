public class UserData : Dictionary<String, User>
{
    public User GetOrCreate(String emailAddress)
    {
        User? data;

        if (!this.TryGetValue(emailAddress, out data))
        {
            data = new User();
            this.Add(emailAddress, data);
        }

        return data;
    }
}