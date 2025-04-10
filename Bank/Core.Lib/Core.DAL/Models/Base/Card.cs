using Bank.DAL.Enums;
using Bank.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Core.DAL.Models.Base
{
    public abstract class Card : BaseEntity
    {
        [Required]
        public string CardNumber { get; set; }
        public CardCategory CardCategory { get; set; }
        public CardType CardType { get; set; }

        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
    }

    public class CardConfiguration : IEntityTypeConfiguration<Card>
    {
        public void Configure(EntityTypeBuilder<Card> builder)
        {
            builder.UseTptMappingStrategy();

            builder.HasIndex(s => s.CardNumber);

            builder.HasOne(s => s.Owner)
                   .WithMany(o => o.Cards)
                   .HasForeignKey(s => s.OwnerId)
                   .IsRequired();
        }
    }
}
