using Azure.Core;

// LDAP variables
String windowsUsername = "";
String windowsPassword = "";
String ldapServer = "10.0.2.8";
int ldapPort = 389;

// Microsoft Graph variables
String clientID = "5e5e74c6-e345-461a-b6d4-e744e5803951";
String tenantID = "e4cc6b96-39df-4494-b0e7-bce164bda677";

// Admin variables
String adminConnectionString = "Server=backup.application.database.production.internal.sawiday.com; Database=rorix_app; Uid=<username>; Pwd=<password>;";

// Data
Dictionary<String, User> userData = new();

MicrosoftGraphAuthentication credentialFetcher = new MicrosoftGraphAuthentication(clientID, tenantID);
TokenCredential microsoftGraphCredentials = credentialFetcher.GetCredentials();

// Fetch users
ActiveDirectoryService adService = new ActiveDirectoryService(windowsUsername, windowsPassword, ldapServer, ldapPort);
adService.ProcessData(userData);

// Fetch admin profiles
AdminService adminService = new AdminService(adminConnectionString);
adminService.ProcessData(userData);

// Fetch license information
LicenseService licenseService = new LicenseService(microsoftGraphCredentials);
licenseService.ProcessData(userData);

Console.WriteLine(userData);
