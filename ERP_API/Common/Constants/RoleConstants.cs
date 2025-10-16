namespace ERP_API.Common.Constants;


public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string User = "User";


    public static readonly string[] AllRoles = { Admin, User };

 
    public static bool IsValidRole(string role) => AllRoles.Contains(role);
}