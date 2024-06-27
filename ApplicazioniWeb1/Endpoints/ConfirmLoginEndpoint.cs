using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ApplicazioniWeb1.Endpoints
{
    public class ConfirmLoginEndpoint
    {
        public static async Task<IResult> Handler(SignInManager<ApplicationUser> signinManager, UserManager<ApplicationUser> userManager)
        {
            var info = await signinManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return Results.Redirect("/external-login");
            }


            var loginUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (loginUser != null)
            {
                var existingUser = await userManager.FindByIdAsync(loginUser.Id);
                await signinManager.SignInAsync(existingUser, true, info.LoginProvider);
                return Results.Redirect("/");
            }

            var user = new ApplicationUser() { UserName = info.Principal.FindFirstValue(ClaimTypes.Email) };
            var createdUserResult = await userManager.CreateAsync(user);

            var result = await userManager.AddLoginAsync(user, info);

            await signinManager.SignInAsync(user, true, info.LoginProvider);

            return Results.Redirect("/confirm-login");
        }
    }
}
