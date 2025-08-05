using System.ComponentModel.DataAnnotations;

namespace FraudFence.Web.Models
{
    public sealed class RegistrationViewModel
    {
        [Required, StringLength(60)]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Choose a password.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
