using Microsoft.AspNetCore.Identity;
using SAGM.Data.Entities;
using SAGM.Models;

namespace SAGM.Helpers
{
    public interface IUserHelper
    {

        Task<User> AddUserAsync(AddUserViewModel model);
        Task<IdentityResult> AddUserAsync(User user, string password);
        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);
        Task<IdentityResult> ChangeUserPasswordAsync(User user, string oldPassword, string newPassword);
        Task CheckRoleAsync(string roleName);
        Task<User> GetUserAsync(string email);
        Task<User> GetUserAsync(Guid userId);
        Task<bool> IsUserInRoleAsync(User user, string roleName);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> UpdateUserAsync(User user);
    }
}
