using Bank.BL.Extensions;
using Bank.BL.Values;
using Core.BL.CQRS.Base;
using Core.BL.Extensions;
using Core.DAL;
using Core.DTO.DTOs.Requests.History;
using Core.DTO.DTOs.Responses.History;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Queries.GetBankAccountsOperationsHistory
{
    public class GetBankAccountsOperationsHistoryHandler : IRequestHandler<MediatorRequest<BankAccountOperationsHistoryQueryDTO, BankAccountOperationsHistoryListDTO>, BankAccountOperationsHistoryListDTO>
    {

        private CoreDbContext _coreDbContext;

        public GetBankAccountsOperationsHistoryHandler(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task<BankAccountOperationsHistoryListDTO> Handle(MediatorRequest<BankAccountOperationsHistoryQueryDTO, BankAccountOperationsHistoryListDTO> request, CancellationToken cancellationToken)
        {
            var operationsHistory = _coreDbContext.BankAccountOperationsHistory
                                    .AsNoTracking()
                                    .OrderByDescending(x => x.OperationDateTime)
                                    .AsQueryable();

            if(request.Request.UsersIds != null && request.Request.UsersIds.Count != 0)
            {
                operationsHistory = operationsHistory.Where(x => x.UserId.HasValue && request.Request.UsersIds.Contains(x.UserId.Value));
            }

            if(request.Request.BankAccountsIds != null && request.Request.BankAccountsIds.Count != 0)
            {
                operationsHistory = operationsHistory.Where(x => request.Request.BankAccountsIds.Contains(x.BankAccountId));
            }

            if (request.Request.StartPeriod.HasValue)
            {
                request.Request.StartPeriod = request.Request.StartPeriod.Value.ToUtcKind();
                operationsHistory = operationsHistory.Where(x => x.OperationDateTime >= request.Request.StartPeriod);
            }

            if (request.Request.EndPeriod.HasValue)
            {
                request.Request.EndPeriod = request.Request.EndPeriod.Value.ToUtcKind();
                operationsHistory = operationsHistory.Where(x => x.OperationDateTime <= request.Request.EndPeriod);
            }

            if (request.Request.BankAccountOperationInitiator.HasValue)
            {
                operationsHistory = operationsHistory.Where(x => x.BankAccountOperationInitiator == request.Request.BankAccountOperationInitiator);
            }

            if (request.Request.BankAccountOperationStatus.HasValue)
            {
                operationsHistory = operationsHistory.Where(x => x.BankAccountOperationStatus == request.Request.BankAccountOperationStatus);
            }

            if (request.Request.BankAccountOperationType.HasValue)
            {
                operationsHistory = operationsHistory.Where(x => x.BankAccountOperationType == request.Request.BankAccountOperationType);
            }

            (var resultList, var metadata) = await operationsHistory.ToPagedListAsync(request.Request.page, 1000);

            return new BankAccountOperationsHistoryListDTO
            {
                BankAccountOperations = resultList.Select(x => x.BankAccountOperationsHistoryToDTO()).ToList(),
                MetaData = metadata
            };
        }
    }
}
