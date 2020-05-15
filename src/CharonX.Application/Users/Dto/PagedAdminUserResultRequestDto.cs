using Abp.Application.Services.Dto;

namespace CharonX.Users.Dto
{
    public class PagedAdminUserResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }

        public bool? IsActive { get; set; }
    }
}
