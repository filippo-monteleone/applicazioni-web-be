using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// The ConfirmLoginEndpoint class handles the confirmation phase after externally logging in.
    /// </summary>
    public class ConfirmLoginEndpoint
    {
        /// <summary>
        /// Handles the external login confirmation process.
        /// </summary>
        /// <param name="signinManager">An instance of <see cref="SignInManager{TUser}"/> used to manage user sign-in operations.</param>
        /// <param name="userManager">An instance of <see cref="UserManager{TUser}"/> used to manage user-related operations.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the login confirmation process.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Retrieves external login information.
        /// 2. If no external login information is found, redirects to the external login page.
        /// 3. Checks if a user associated with the external login exists.
        ///    - If the user exists, signs in the user and redirects to the home page.
        ///    - If the user does not exist, creates a new user, associates the external login with the new user, signs in the user, and redirects to the confirm login page.
        /// </remarks>
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
