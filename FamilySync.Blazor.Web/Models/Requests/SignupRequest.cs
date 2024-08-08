using System.ComponentModel.DataAnnotations;

namespace FamilySync.Blazor.Web.Models.Requests;

public sealed record SignupRequest
{
    [EmailAddress] 
    [Required]
    public string Email { get; set; } = default!;
    
    [Required]
    public string Username { get; set; } = default!;
    
    [Required]
    public string Password { get; set; } = default!;
}