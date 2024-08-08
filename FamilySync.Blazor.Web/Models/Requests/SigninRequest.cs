using System.ComponentModel.DataAnnotations;

namespace FamilySync.Blazor.Web.Models.Requests;

public sealed record SigninRequest
{
    [Required] [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}