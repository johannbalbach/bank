namespace UserService.Dtos
{
    public class UserBrieflyPaginationListDto
    {
        public List<UserBrieflyDto> users { get; set; } = new List<UserBrieflyDto>();
        public int totalCount { get; set; }
        public int pageSize { get; set; }
        public int pageIndex { get; set; }
    }
}
