namespace StorageService.Dto
{
    public class CreateConfigResponse
    {
        public Guid UserId { get; set; }
        public string Device { get; set; }
        public string Config { get; set; }
    }
}
