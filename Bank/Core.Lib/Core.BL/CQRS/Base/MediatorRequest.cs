using Bank.DAL.Enums;
using MediatR;

namespace Core.BL.CQRS.Base
{
    public class MediatorRequest<TRequest, TResponse> : IRequest<TResponse>
        where TRequest : class
        where TResponse : class
    {
        public TRequest Request { get; set; }
        public MediatorMetaData? MetaData { get; set; } = null!;
    }

    public class MediatorMetaData
    {
        public Guid UserId { get; set; }
        public UserRole UserRole { get; set; }
    }
}
