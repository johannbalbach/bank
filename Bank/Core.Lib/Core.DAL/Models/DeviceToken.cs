using Bank.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Core.DAL.Models
{
    public class DeviceToken
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public UserRole UserRole { get; set; }
        [Required]
        public string DeviceName { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
    {
        public void Configure(EntityTypeBuilder<DeviceToken> builder)
        {
            builder.HasKey(x => new { x.UserId, x.DeviceName });
        }
    }
}
