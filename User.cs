public class User
{
    public String? FunctionGroup { get; set; }
    public String? FirstName { get; set; }
    public String? LastName { get; set; }
    public String? WindowsUsername { get; set; }
    public String? Title { get; set; }
    public HashSet<String> Licenses { get; } = new HashSet<String>();
    public HashSet<String> AdminProfiles { get; } = new HashSet<String>();
    public String? Webstock { get; set; }
    public Dictionary<String, HashSet<String>> Youtrack { get; } = new();
}
