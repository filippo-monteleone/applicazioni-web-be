using ApplicazioniWeb1.Data;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Endpoints
{
    public class RoleEndpoint
    {
        public class InviteForm
        {
            public string Invite { get; set; }
        }

        public static async Task<IResult> Handler(UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            var roles = await userManager.GetRolesAsync(user);

            return roles.Count() == 0 ? Results.NotFound() : Results.Ok(roles);
        }

        public static async Task<IResult> PostHandler(InviteForm invite, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            if (invite.Invite == "admin")
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "admin" });
                await userManager.AddToRoleAsync(user, "admin");
            } else
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "user" });
                await userManager.AddToRoleAsync(user, "user");
            }
            return Results.Ok();
        }
    }
}
