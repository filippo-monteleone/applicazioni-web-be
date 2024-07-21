using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// The RegisterEndpoint class handles user registration requests.
    /// </summary>
    public class RegisterEndpoint
    {
        /// <summary>
        /// Represents the registration form data.
        /// </summary>
        public class RegisterForm
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        /// Handles the user registration request.
        /// </summary>
        /// <param name="form">An instance of <see cref="RegisterForm"/> containing the username and password.</param>
        /// <param name="invite">An optional invite code used to determine the role of the user.</param>
        /// <param name="userManager">An instance of <see cref="UserManager{TUser}"/> used to manage user-related operations.</param>
        /// <param name="signInManager">An instance of <see cref="SignInManager{TUser}"/> used to manage user sign-in operations.</param>
        /// <param name="roleManager">An instance of <see cref="RoleManager{TRole}"/> used to manage user roles.</param>
        /// <returns>A <see cref="Results{TResult1, TResult2}"/> indicating the outcome of the registration operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Creates a new user with the provided username and password.
        /// 2. Assigns a role to the user based on the invite code:
        ///    - If the invite code is "admin", assigns the "admin" role.
        ///    - Otherwise, assigns the "user" role.
        /// 3. If user creation fails, returns a Bad Request result.
        /// 4. If user creation succeeds, signs in the user and returns a Created result.
        /// </remarks>
        public static async Task<Results<Created, BadRequest>> Handler(RegisterForm form, string? invite, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
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
                return TypedResults.BadRequest();

            await signInManager.SignInAsync(user, true);

            return TypedResults.Created();
        }
    }
}
