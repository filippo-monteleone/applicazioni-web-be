using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// The LogoutEndpoint class handles user logout requests.
    /// </summary>
    public class LogoutEndpoint
    {
        /// <summary>
        /// Handles the user logout request.
        /// </summary>
        /// <param name="signInManager">An instance of <see cref="SignInManager{TUser}"/> used to manage user sign-in operations.</param>
        /// <returns>An <see cref="Ok"/> result indicating the logout operation was successful.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Signs out the current user.
        /// 2. Returns an OK result to indicate successful logout.
        /// </remarks>
        [Authorize]
        public static async Task<Ok> Handler(SignInManager<ApplicationUser> signInManger)
        {
            await signInManger.SignOutAsync();
            return TypedResults.Ok();
        }
    }
}
