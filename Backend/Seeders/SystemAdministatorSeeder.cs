using Microsoft.AspNetCore.Identity;
using Server.Models;

namespace Server.Seeders
{
    public class SystemAdministatorSeeder
    {
        public RoleManager<IdentityUser> RoleManager { get; set; }
        public UserManager<IdentityUser> UserManager { get; set; }

        public SystemAdministatorSeeder(RoleManager<IdentityUser> roleManager, UserManager<IdentityUser> userManager)
        {
            RoleManager = roleManager;
            UserManager = userManager;
        }

        public async void Run()
        {
            User systemAdministratorCreaditals = new()
            {
                UserName = "System Administrator",
                Email = "admin@example.com",
                PhoneNumber = "+375299999999"
            };

            await UserManager.CreateAsync(systemAdministratorCreaditals, "default@Admin321");
            await UserManager.AddToRoleAsync(systemAdministratorCreaditals, UserRoles.SysAdmin);            
        }
    }
}
