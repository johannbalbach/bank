using Core.BL.CQRS.Base;
using Core.BL.CQRS.Queries.GetUserBankAccounts;
using Core.DTO.DTOs.Responses.Aggregates;
using MediatR;

namespace Core.BL.CQRS.Queries.GetMyBankAccounts
{
    public class GetMyBankAccountsRequestHandler : IRequestHandler<MediatorRequest<GetMyBankAccountsRequest, UserBankAccountsResponseDTO>, UserBankAccountsResponseDTO>
    {
        private readonly IMediator _mediator;

        public GetMyBankAccountsRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<UserBankAccountsResponseDTO> Handle(MediatorRequest<GetMyBankAccountsRequest, UserBankAccountsResponseDTO> request, CancellationToken cancellationToken)
        {
            GetUserBankAccountsRequest userRequest = new()
            {
                UserId = request.MetaData!.UserId,
                AccountsIds = request.Request.AccountsIds
            };

            return await _mediator.Send(new MediatorRequest<GetUserBankAccountsRequest, UserBankAccountsResponseDTO> { Request = userRequest }, cancellationToken);
        }
    }
}
