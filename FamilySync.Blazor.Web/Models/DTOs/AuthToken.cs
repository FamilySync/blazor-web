namespace FamilySync.Blazor.Web.Models.DTOs;

public record AuthToken(string AccessToken, int ExpiresIn, string Type, string CookieKey, DateTime RefreshTokenExpiryDate);