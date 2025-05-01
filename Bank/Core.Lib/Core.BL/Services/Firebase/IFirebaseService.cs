using Core.BL.CQRS.Base;

namespace Core.BL.Services.Firebase
{
    public interface IFirebaseService
    {
        public Task SendFirebasePushMessage(MediatorMetaData metaData, FirebaseNotificationMessage aggregate);
    }
}
