using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// The LoginEndpoint class handles user login requests.
    /// </summary>
    public class LoginEndpoint
    {
        /// <summary>
        /// Represents the login form data.
        /// </summary>
        public class LoginForm
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        /// Handles the login request by validating the provided username and password.
        /// </summary>
        /// <param name="login">An instance of <see cref="LoginForm"/> containing the username and password.</param>
        /// <param name="signInManager">An instance of <see cref="SignInManager{TUser}"/> used to manage user sign-in operations.</param>
        /// <returns>A <see cref="Results{TResult1, TResult2}"/> indicating the outcome of the login operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Attempts to sign in the user with the provided username and password.
        /// 2. If the sign-in is successful, returns an OK result.
        /// 3. If the sign-in fails, returns a Bad Request result.
        /// </remarks>
        public static async Task<Results<Ok, BadRequest>> Handler([FromBody] LoginForm login, SignInManager<ApplicationUser> signInManager)
        {
            var result = await signInManager.PasswordSignInAsync(login.Username, login.Password, true, false);

            if (result.Succeeded)
            {
                return TypedResults.Ok();
            }

            return TypedResults.BadRequest();
        }
    }
}
