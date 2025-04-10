using Bank.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Dtos.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public UserRole Role { get; set; }
        public string ReturnUrl { get; set; }
    }
}
