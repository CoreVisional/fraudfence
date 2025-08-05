using FraudFence.EntityModels.Dto;
using FraudFence.EntityModels.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System;

namespace FraudFence.Service
{
    public sealed class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<(IdentityResult Result, ApplicationUser? User)> CreateAsync(RegistrationDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return (result, null);

            await _userManager.AddToRoleAsync(user, "Consumer");

            await _userManager.AddClaimsAsync(user, [new Claim("FullName", user.Name), new Claim(ClaimTypes.Email, user.Email!)]);

            return (result, user);
        }

        public async Task<List<UserViewModel>> GetUsersAsync(string search = null, string role = null)
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userViewModels.Add(new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = !u.LockoutEnabled || (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now),
                    Roles = roles.ToList()
                });
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                userViewModels = userViewModels.Where(u =>
                    (u.Name != null && u.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Email != null && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(role)).ToList();
            }
            return userViewModels;
        }

        public List<string> GetAllRoles(bool excludeAdmin = false)
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            if (excludeAdmin)
                roles = roles.Where(r => r != "Admin").ToList();
            return roles;
        }

        public async Task<(IdentityResult Result, ApplicationUser? User)> CreateUserAsync(CreateUserViewModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                var error = IdentityResult.Failed(new IdentityError { Description = "A user with this email already exists." });
                return (error, null);
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return (result, null);
            await _userManager.AddToRoleAsync(user, model.Role);
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", user.Name));
            return (result, user);
        }

        public async Task<EditUserViewModel?> GetEditUserViewModelAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;
            return new EditUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<IdentityResult> EditUserAsync(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return IdentityResult.Failed(new IdentityError { Description = "A user with this email already exists." });
            }
            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            return await _userManager.UpdateAsync(user);
        }

        public async Task ToggleActiveAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return;
            if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.Now)
            {
                // Activate
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                // Deactivate
                user.LockoutEnd = DateTimeOffset.MaxValue;
                user.LockoutEnabled = true;
            }
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return;
            await _userManager.DeleteAsync(user);
        }
    }
}
