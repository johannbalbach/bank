using Core.DTO.DTOs.Other;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.DTO.DTOs.Responses.Cards;
using X.PagedList;

namespace Core.DTO.DTOs.Responses.BankAccounts
{
    public class BankAccountsListDTO
    {
        public List<BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>> BankAccounts { get; set; }
        public PagedListMetaDataNew MetaData { get; set; }
    }
}
