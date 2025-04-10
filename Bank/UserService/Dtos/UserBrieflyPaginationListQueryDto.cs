using Bank.DAL.Enums;
using UserService.Dtos.Enums;

namespace UserService.Dtos
{
    public class UserBrieflyPaginationListQueryDto
    {
        public UserRole? userRole {  get; set; }
        public bool? isBlocked { get; set; }
        public UserBrieflyListSortByEnum sortBy { get; set; } = UserBrieflyListSortByEnum.ByUserNameAsc;
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
    }
}
