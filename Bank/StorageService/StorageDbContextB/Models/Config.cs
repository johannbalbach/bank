using Bank.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace StorageService.StorageDbContextB.Models
{
    public class Config : BaseEntity
    {
        public Guid UserId { get; set; }
        [Required]
        public string Device { get; set; }
        [Required]
        public string Configuration { get; set; }
    }
}
