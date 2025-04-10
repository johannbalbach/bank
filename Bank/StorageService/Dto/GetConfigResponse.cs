namespace StorageService.Dto
{
    public class GetConfigResponse
    {
        public Guid UserId { get; set; }
        public string Device { get; set; }
        public string Config { get; set; }
    }
}
