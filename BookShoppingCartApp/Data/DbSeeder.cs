using BookShoppingCartApp.Constants;
using Microsoft.AspNetCore.Identity;

namespace BookShoppingCartApp.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider serviceProvider)
        {
            var userMgr = serviceProvider.GetService<UserManager<IdentityUser>>();
            var roleMgr = serviceProvider.GetService<RoleManager<IdentityRole>>();

            await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));

            var admin = new IdentityUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true
            };

            var userInDb = await userMgr.FindByEmailAsync(admin.Email);
            if (userInDb is null)
            {
                await userMgr.CreateAsync(admin, "Admin@123");
                await userMgr.AddToRoleAsync(admin, Roles.Admin.ToString());
            }
        }
    }
}
