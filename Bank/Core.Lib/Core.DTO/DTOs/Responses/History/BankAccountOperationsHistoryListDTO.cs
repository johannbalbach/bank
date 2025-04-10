using Core.DTO.DTOs.Other;
using X.PagedList;

namespace Core.DTO.DTOs.Responses.History
{
    public class BankAccountOperationsHistoryListDTO
    {
        public List<BankAccountOperationsHistoryResponseDTO> BankAccountOperations { get; set; }
        public PagedListMetaDataNew MetaData { get; set; }
    }
}
