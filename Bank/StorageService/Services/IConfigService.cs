using StorageService.Dto;

namespace StorageService.Services
{
    public interface IConfigService
    {
        public Task<CreateConfigResponse> CreateConfig(CreateConfigRequest request, Guid userId);
        public Task<GetConfigResponse> GetConfig(GetConfigRequest request, Guid userId);
        public Task RemoveConfig(DeleteConfigRequest request, Guid userId);
    }
}
