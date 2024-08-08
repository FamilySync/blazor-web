namespace FamilySync.Blazor.Web.Models.DTOs;

public record AuthTokenDTO(string AccessToken, int ExpiresIn, string Type, string CookieKey, DateTime RefreshTokenExpiryDate);