using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using CharonX.Authorization.Users;

namespace CharonX.Sessions.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserLoginInfoDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public string[] Roles { get; set; }

        public string[] OrgUnits { get; set; }

        public string[] Permissions { get; set; }

        public bool IsAdmin { get; set; }
    }
}
