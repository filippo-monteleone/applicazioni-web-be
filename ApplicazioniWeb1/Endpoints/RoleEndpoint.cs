using ApplicazioniWeb1.Data;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Metadata.Ecma335;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// The RoleEndpoint class handles role-related requests for users.
    /// </summary>
    public class RoleEndpoint
    {
        /// <summary>
        /// Represents the invite form data.
        /// </summary>
        public class InviteForm
        {
            public string Invite { get; set; }
        }

        /// <summary>
        /// Handles the request to get the roles of the current user.
        /// </summary>
        /// <param name="userManager">An instance of <see cref="UserManager{TUser}"/> used to manage user-related operations.</param>
        /// <param name="ctx">The current <see cref="HttpContext"/> instance.</param>
        /// <returns>A <see cref="Results{TResult1, TResult2}"/> indicating the outcome of the request, either the user's roles or a NotFound result.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Retrieves the current user from the HttpContext.
        /// 2. If the user is not found, returns a NotFound result.
        /// 3. If the user is found, retrieves the user's roles.
        /// 4. If no roles are found, returns a NotFound result.
        /// 5. If roles are found, returns an OK result with the roles.
        /// </remarks>
        [Authorize]
        public static async Task<Results<Ok<IList<string>>, NotFound>> Handler(UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            if (user is null)
                return TypedResults.NotFound();

            var roles = await userManager.GetRolesAsync(user);
            return roles.Count == 0 ? TypedResults.NotFound() : TypedResults.Ok(roles);
        }

        /// <summary>
        /// Handles the request to add a role to the current user based on the provided invite code.
        /// </summary>
        /// <param name="invite">An instance of <see cref="InviteForm"/> containing the invite code or role name.</param>
        /// <param name="userManager">An instance of <see cref="UserManager{TUser}"/> used to manage user-related operations.</param>
        /// <param name="roleManager">An instance of <see cref="RoleManager{TRole}"/> used to manage roles.</param>
        /// <param name="signInManager">An instance of <see cref="SignInManager{TUser}"/> used to manage user sign-in operations.</param>
        /// <param name="ctx">The current <see cref="HttpContext"/> instance.</param>
        /// <returns>A <see cref="Results{TResult1, TResult2}"/> indicating the outcome of the request, either OK or NotFound.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Retrieves the current user from the HttpContext.
        /// 2. Creates the role specified in the invite code if it does not already exist.
        /// 3. Assigns the role to the user.
        /// 4. Refreshes the user's sign-in.
        /// 5. Returns an OK result to indicate successful role assignment.
        /// </remarks>
        [Authorize]
        public static async Task<Results<Ok, NotFound>> PostHandler(InviteForm invite, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            await roleManager.CreateAsync(new IdentityRole() { Name = invite.Invite });
            await userManager.AddToRoleAsync(user, invite.Invite);

            await signInManager.RefreshSignInAsync(user);

            return TypedResults.Ok();
        }
    }
}
