using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Features.Dto;
using CharonX.Roles.Dto;
using CharonX.Users.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharonX.Roles
{
    [AbpAuthorize(PermissionNames.Pages_Roles)]
    public class RoleAppService : AsyncCrudAppService<Role, RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>, IRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;

        public RoleAppService(
            IRepository<Role> repository, 
            RoleManager roleManager,
            UserManager userManager)
            : base(repository)
        {
            _roleManager = roleManager;
            _userManager = userManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        public override async Task<RoleDto> CreateAsync(CreateRoleDto input)
        {
            CheckCreatePermission();

            var role = ObjectMapper.Map<Role>(input);
            role.SetNormalizedName();

            // CheckErrors(await _roleManager.CreateAsync(role));
            //
            // var grantedPermissions = PermissionManager
            //     .GetAllPermissionsInSystem()
            //     .Where(p => input.GrantedPermissions.Contains(p.Name))
            //     .ToList();
            //
            // await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            await _roleManager.CreateAndGrantPermissionAsync(role, input.GrantedPermissions);

            return MapToEntityDto(role);
        }

        public async Task<ListResultDto<RoleListDto>> GetRolesByPermissionAsync(GetRolesInput input)
        {
             var roles = await _roleManager
                .Roles
                .WhereIf(
                    !input.Permission.IsNullOrWhiteSpace(),
                    r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                )
                .ToListAsync();

            return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
        }

        public async Task AddUserToRoleAsync(SetRoleUserDto input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }

        public async Task RemoveUserFromRoleAsync(SetRoleUserDto input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }

        public async Task<List<UserDto>> GetUsersInRoleAsync(EntityDto<int> input)
        {
            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            if (role == null)
            {
                throw new UserFriendlyException(L("RoleNotFound", input.Id));
            }

            var users = await _userManager.GetUsersInRoleAsync(role.Name);

            return ObjectMapper.Map<List<UserDto>>(users);
        }

        public ListResultDto<PermissionDto> GetAllAvailablePermissions()
        {
            var permissions = PermissionManager.GetAllPermissions();

            return new ListResultDto<PermissionDto>(
                ObjectMapper.Map<List<PermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList()
            );
        }

        public override async Task<RoleDto> UpdateAsync(RoleDto input)
        {
            CheckUpdatePermission();

            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            ObjectMapper.Map(input, role);

            //CheckErrors(await _roleManager.UpdateAsync(role));

            //var grantedPermissions = PermissionManager
            //    .GetAllPermissionsInSystem()
            //    .Where(p => input.GrantedPermissions.Contains(p.Name))
            //    .ToList();

            //await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            await _roleManager.UpdateRoleAndPermissionAsync(role, input.GrantedPermissions);

            return MapToEntityDto(role);
        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var role = await _roleManager.FindByIdAsync(input.Id.ToString());
            if (role == null)
            {
                throw new UserFriendlyException(L("RoleNotFound", input.Id));
            }
            
            // var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);
            //
            // foreach (var user in users)
            // {
            //     CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.NormalizedName));
            // }
            //
            // CheckErrors(await _roleManager.DeleteAsync(role));

            await _roleManager.DeleteRoleAndDetachUserAsync(role);
        }

        protected override IQueryable<Role> CreateFilteredQuery(PagedRoleResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Permissions)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword)
                || x.DisplayName.Contains(input.Keyword)
                || x.Description.Contains(input.Keyword));
        }

        protected override async Task<Role> GetEntityByIdAsync(int id)
        {
            return await Repository.GetAllIncluding(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override IQueryable<Role> ApplySorting(IQueryable<Role> query, PagedRoleResultRequestDto input)
        {
            return query.OrderBy(r => r.DisplayName);
        }

        public async Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input)
        {
            var permissions = PermissionManager.GetAllPermissions();
            var role = await _roleManager.GetRoleByIdAsync(input.Id);
            var grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
            var roleEditDto = ObjectMapper.Map<RoleEditDto>(role);

            return new GetRoleForEditOutput
            {
                Role = roleEditDto,
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList(),
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
            };
        }
    }
}

