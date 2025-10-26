namespace ERP_API.Common.Configuration;

public class AdminSettings
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string DefaultPassword { get; set; } = default!;  
    public bool ForcePasswordChange { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new InvalidOperationException("Admin Email no está configurado");

        if (string.IsNullOrWhiteSpace(DefaultPassword))
            throw new InvalidOperationException("Admin Password no está configurado");

        if (DefaultPassword.Length < 8)
            throw new InvalidOperationException("Admin Password debe tener al menos 8 caracteres");
    }
}