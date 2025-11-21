namespace ERP_API.Common.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Accountant = "Accountant";
    public const string WarehouseManager = "WarehouseManager";
    public const string SalesRepresentative = "SalesRepresentative";

    public static readonly string[] AllRoles =
    {
        Admin,
        User,
        Manager,
        Accountant,
        WarehouseManager,
        SalesRepresentative
    };

    public static bool IsValidRole(string role) => AllRoles.Contains(role);

    public static class Permissions
    {
        public static readonly string[] AdminRoles = { Admin };
        public static readonly string[] FinanceRoles = { Admin, Manager, Accountant };
        public static readonly string[] InventoryRoles = { Admin, Manager, WarehouseManager };
        public static readonly string[] SalesRoles = { Admin, Manager, SalesRepresentative };
    }

    /// <summary>
    /// Roles con acceso completo al dashboard
    /// </summary>
    public static readonly string[] DashboardFullAccess =
    {
        Admin,
        Manager
    };

    /// <summary>
    /// Roles con acceso limitado al dashboard (solo inventario)
    /// </summary>
    public static readonly string[] DashboardInventoryAccess =
    {
        WarehouseManager  
    };
}