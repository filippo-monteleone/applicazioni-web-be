using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicazioniWeb1.Endpoints
{
    public class RegisterEndpoint
    {
       public class RegisterForm
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public static async Task<IResult> Handler(RegisterForm form, string? invite, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            var user = new ApplicationUser() { UserName = form.Username };
            var createUserResult = await userManager.CreateAsync(user, form.Password);

            if (invite == "admin")
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "admin" });
                await userManager.AddToRoleAsync(user, "admin");
            } else
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "user" });
                await userManager.AddToRoleAsync(user, "user");
            }

            if (!createUserResult.Succeeded)
                return Results.BadRequest();

            await signInManager.SignInAsync(user, true);

            return Results.Ok();
        }
    }
}
