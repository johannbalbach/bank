using Bank.DAL.Enums;
using Core.BL.CQRS.Base;
using Core.DAL;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.Services.Firebase
{
    public class FirebaseNotificationMessage
    {
        public Notification UserNotification { get; set; }
        public Notification EmployeesNotification { get; set; }
    }

    public class FirebaseService : IFirebaseService
    {
        private readonly CoreDbContext _context;

        private readonly FirebaseMessaging _default;

        public FirebaseService(CoreDbContext context)
        {
            _context = context;
            _default = FirebaseMessaging.DefaultInstance;
        }

        public async Task SendFirebasePushMessage(MediatorMetaData metaData, FirebaseNotificationMessage aggregate)
        {
            await SendUserMessage(metaData.UserId, metaData.DeviceName, aggregate.UserNotification);
            await SendEmployeesMessages(aggregate.EmployeesNotification);
        }

        private async Task SendUserMessage(Guid userId, string deviceName, Notification notification)
        {
            var token = await _context.DeviceTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceName == deviceName);

            if(token == null)
            {
                return;
            }

            Message message = new()
            {
                Token = token.Token,
                Notification = notification
            };

            var result = await _default.SendAsync(message);
        }

        private async Task SendEmployeesMessages(Notification notification)
        {
            var tokens = await _context.DeviceTokens
                        .Where(x => x.UserRole == UserRole.Employee)
                        .Select(x => x.Token)
                        .ToListAsync();

            MulticastMessage multicast = new()
            {
                Tokens = tokens,
                Notification = notification
            };

            var result = await _default.SendEachForMulticastAsync(multicast);
        }
    }
}
