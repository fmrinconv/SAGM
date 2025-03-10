﻿using Microsoft.AspNetCore.Identity;
using SAGM.Data;
using SAGM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SAGM.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.Rendering;
using Humanizer;
using System.Collections.Immutable;
using SAGM.Enums;

namespace SAGM.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly SAGMContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UserHelper(SAGMContext context, 
                          UserManager<User> userManager, 
                          RoleManager<IdentityRole> roleManager, 
                          SignInManager<User> signInManager) 
        { 
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User> AddUserAsync(AddUserViewModel model)
        {
            User user = new()
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ImageId = model.ImageId,
                PhoneNumber = model.PhoneNumber,
                City = await _context.Cities.FindAsync(model.CityId),
                UserName = model.Username,


            };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            User newUser = await GetUserAsync(model.Username);
            await AddUserToRoleAsync(newUser, user.UserType.ToString());
            return newUser;
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);

        }

        public async Task RemoveFromRoleAsync(User user, string roleName) { 

            await _userManager.RemoveFromRoleAsync(user, roleName);

        }

        public async Task<IdentityResult> ChangeUserPasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole 
                { 
                    Name = roleName 
                });
            }
        }

        public async Task<User> GetUserAsync(string email)
        {
            return await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.State)
                .ThenInclude(s => s.Country)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.State)
                .ThenInclude(s => s.Country)
                .FirstOrDefaultAsync(u => u.Id == userId.ToString());
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
           
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, true);
            
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);

        
        }


        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);   
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user) 
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }


        public async Task<IEnumerable<string>> GetUserRolesAsync(User user)
        {
     
            IList<string> list =  await _userManager.GetRolesAsync(user);
            return list;

        }

        public async Task<IEnumerable<string>> GetRolesForUserAsync(User user)
        {

            IList<string> listAllRoles = new List<string>();

            foreach (var rol in Enum.GetValues(typeof(Enums.UserType)))
            {
                listAllRoles.Add(rol.ToString());
            }

            IList<string> listAsignedRoles = await _userManager.GetRolesAsync(user);

            IList<string> listRolesForUser = new List<string>();

            listRolesForUser = (IList<string>)listAllRoles.Except(listAsignedRoles).ToList();

            return listRolesForUser;

        }

        public async Task<IEnumerable<SelectListItem>> GetSellersAsync()
        {
            List<User> list = await _userManager.Users.ToListAsync();
            List<SelectListItem> listSellers = new List<SelectListItem>();

            foreach (User user in list)
            {
                if (await _userManager.IsInRoleAsync(user, "Vendedor"))
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = user.FullName;
                    item.Value = user.UserName;

                    listSellers.Add(item);
                }
            }
            return listSellers;

        }

        public async Task<IEnumerable<SelectListItem>> GetBuyersAsync()
        {
            List<User> list = await _userManager.Users.ToListAsync();
            List<SelectListItem> listBuyers = new List<SelectListItem>();

            foreach (User user in list)
            {
                if (await _userManager.IsInRoleAsync(user, "Comprador"))
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = user.FullName;
                    item.Value = user.UserName;

                    listBuyers.Add(item);
                }
            }
            return listBuyers;

        }

        public async Task<IEnumerable<SelectListItem>> GetAllUsersAsync()
        {
            List<User> list = await _userManager.Users.ToListAsync();

            List<SelectListItem> listusers = new List<SelectListItem>();

            foreach (User user in list)
            {

                    SelectListItem item = new SelectListItem();
                    item.Text = user.FullName;
                    item.Value = user.UserName;

                listusers.Add(item);

            }
            return listusers;

        }

        public async Task<IEnumerable<SelectListItem>> GetUsersByRoleAsync(UserType role)
        {
            List<User> list = await _userManager.Users.ToListAsync();
            List<SelectListItem> listReceptors = new List<SelectListItem>();

            foreach (User user in list)
            {
                if (await _userManager.IsInRoleAsync(user, role.ToString()))
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = user.FullName;
                    item.Value = user.UserName;

                    listReceptors.Add(item);
                }
            }
            return listReceptors;
        }

        public Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool x, bool y)
        {
            throw new NotImplementedException();
        }
    }
}
