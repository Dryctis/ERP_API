namespace ERP_API.Common.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Secret { get; set; } = default!;  
    public int AccessMinutes { get; set; } = 30;
    public int RefreshDays { get; set; } = 7;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret))
            throw new InvalidOperationException("JWT Secret no está configurado");

        if (Secret.Length < 32)
            throw new InvalidOperationException("JWT Secret debe tener al menos 32 caracteres");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT Issuer no está configurado");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT Audience no está configurado");
    }
}