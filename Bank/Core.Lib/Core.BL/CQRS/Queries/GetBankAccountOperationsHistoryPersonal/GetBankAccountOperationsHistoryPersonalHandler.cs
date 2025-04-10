using Core.BL.CQRS.Base;
using Core.DAL;
using Core.DTO.DTOs.Requests.History;
using Core.DTO.DTOs.Responses.History;
using MediatR;

namespace Core.BL.CQRS.Queries.GetBankAccountOperationsHistoryPersonal
{
    public class GetBankAccountOperationsHistoryPersonalHandler : IRequestHandler<MediatorRequest<BankAccountOperationsHistoryQueryPersonalDTO, BankAccountOperationsHistoryListDTO>, BankAccountOperationsHistoryListDTO>
    {
        private IMediator _mediator;

        public GetBankAccountOperationsHistoryPersonalHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<BankAccountOperationsHistoryListDTO> Handle(MediatorRequest<BankAccountOperationsHistoryQueryPersonalDTO, BankAccountOperationsHistoryListDTO> request, CancellationToken cancellationToken)
        {
            var query = new BankAccountOperationsHistoryQueryDTO
            {
                UsersIds = new List<Guid>() { request.MetaData!.UserId },
                page = request.Request.page,
                StartPeriod = request.Request.StartPeriod,
                EndPeriod = request.Request.EndPeriod,
                BankAccountOperationInitiator = request.Request.BankAccountOperationInitiator,
                BankAccountOperationStatus = request.Request.BankAccountOperationStatus,
                BankAccountOperationType = request.Request.BankAccountOperationType,
                BankAccountsIds = request.Request.BankAccountsIds
            };

            return await _mediator.Send(new MediatorRequest<BankAccountOperationsHistoryQueryDTO, BankAccountOperationsHistoryListDTO>
            {
                Request = query
            });
        }
    }
}
