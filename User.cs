public class User
{
    public String? FirstName { get; set; }
    public String? LastName { get; set; }
    public String? WindowsUsername { get; set; }
    public String? Title { get; set; }
    public List<String> Licenses { get; set; } = new List<String>();
    public List<String> AdminProfiles { get; set; } = new List<String>();
    public String? Webstock { get; set; }
}
