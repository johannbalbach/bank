using Bank.DAL.Enums;
using Bank.DTO.DTOs;

namespace Core.DTO.DTOs.Responses.Cards
{
    public class CardResponseDTO : BaseDTO
    {
        public string CardNumber { get; set; }
        public CardCategory CardCategory { get; set; }
        public CardType CardType { get; set; }
    }
}
