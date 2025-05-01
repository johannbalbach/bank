using Core.BL.CQRS.Base;
using Core.DAL;
using Core.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Commands.StoreDeviceToken
{
    public class StoreDeviceTokenRequestHandler : IRequestHandler<MediatorRequest<StoreDeviceTokenRequest, StoreDeviceTokenResponse>, StoreDeviceTokenResponse>
    {
        private readonly CoreDbContext _context;

        public StoreDeviceTokenRequestHandler(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<StoreDeviceTokenResponse> Handle(MediatorRequest<StoreDeviceTokenRequest, StoreDeviceTokenResponse> request, CancellationToken cancellationToken)
        {
            StoreDeviceTokenRequest tokenRequest = request.Request;
            MediatorMetaData mediatorMetaData = request.MetaData!;

            if(mediatorMetaData.DeviceName == null)
            {
                throw new Exception("Could not find device name header");
            }

            var tokenFromDb = await _context.DeviceTokens.FirstOrDefaultAsync(x => x.UserId == mediatorMetaData.UserId && x.DeviceName == mediatorMetaData.DeviceName, cancellationToken: cancellationToken);

            if (tokenFromDb == null)
            {
                DeviceToken newToken = new()
                {
                    Token = tokenRequest.Token,
                    UserId = mediatorMetaData.UserId,
                    UserRole = mediatorMetaData.UserRole,
                    DeviceName = mediatorMetaData.DeviceName
                };

                await _context.DeviceTokens.AddAsync(newToken, cancellationToken);
            }
            else
            {
                tokenFromDb.Token = tokenRequest.Token;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new StoreDeviceTokenResponse();
        }
    }
}
