using ApplicazioniWeb1.Data;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Metadata.Ecma335;

namespace ApplicazioniWeb1.Endpoints
{
    public class RoleEndpoint
    {
        public class InviteForm
        {
            public string Invite { get; set; }
        }

        [Authorize]
        public static async Task<Results<Ok<IList<string>>, NotFound>> Handler(UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            if (user is null)
                return TypedResults.NotFound();

            var roles = await userManager.GetRolesAsync(user);
            return roles.Count == 0 ? TypedResults.NotFound() : TypedResults.Ok(roles);
        }

        [Authorize]
        public static async Task<Results<Ok, NotFound>> PostHandler(InviteForm invite, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            if (user is null)
                return TypedResults.NotFound();

            if (invite.Invite == "admin")
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "admin" });
                await userManager.AddToRoleAsync(user, "admin");
            } else
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "user" });
                await userManager.AddToRoleAsync(user, "user");
            }

            await signInManager.RefreshSignInAsync(user);

            return TypedResults.Ok();
        }
    }
}
