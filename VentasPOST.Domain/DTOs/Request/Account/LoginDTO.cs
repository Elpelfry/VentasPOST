using System.ComponentModel.DataAnnotations;

namespace VentasPOST.Domain.DTOs.Request.Account;

public class LoginDTO
{
    [EmailAddress, Required]
    [RegularExpression("[^@ \\t\\r\\n]+@[^@ \\t\\r\\n]+\\.[^@ \\t\\r\\n]+",
        ErrorMessage = "Your Email is not valid, provide valid email such @gmail")]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$",
        ErrorMessage = "Your password must be a mix od Alphanumeric and special characters")]
    public string Password { get; set; } = string.Empty;
}
