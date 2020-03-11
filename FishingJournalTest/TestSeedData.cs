using System;
using System.Linq;
using FishingJournal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FishJournalTest
{
    public static class TestSeedData
    {
        public const string TestEmail = "justin@rodenbostel.com";
        public const string TestPassword = "Test_password1234";

        public static void Initialize(IServiceProvider serviceProvider)
        {
            SeedRoles(serviceProvider);
            SeedUsers(serviceProvider);
        }

        private static void SeedUsers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            foreach (var user in userManager.Users.ToList())
            {
                userManager.DeleteAsync(user).Wait();
            }

            if (userManager.FindByEmailAsync(TestEmail).Result == null)
            {
                var user = new User {UserName = TestEmail, Email = TestEmail, EmailConfirmed = true};

                var result = userManager.CreateAsync(user, TestPassword).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }
        }

        private static void SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager
                = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in roleManager.Roles.ToList())
            {
                roleManager.DeleteAsync(role).Wait();
            }

            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                var role = new IdentityRole {Name = "Admin"};
                var roleResult = roleManager.CreateAsync(role).Result;
            }
        }
    }
}