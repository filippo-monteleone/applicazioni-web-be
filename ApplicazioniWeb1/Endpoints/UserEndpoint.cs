using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ApplicazioniWeb1.Endpoints
{
    public class UserForm
    {
        public float? Balance { get; set; }
        public bool? Pro { get; set; }
        public int? Battery { get; set; }
    }

    /// <summary>
    /// The UserEndpoint class handles user-related requests.
    /// </summary>
    public class UserEndpoint
    {
        /// <summary>
        /// Handles the request to get the details of the current user.
        /// </summary>
        /// <param name="ctx">The current <see cref="HttpContext"/> instance.</param>
        /// <param name="userManager">An instance of <see cref="UserManager{TUser}"/> used to manage user-related operations.</param>
        /// <returns>A <see cref="Results{TResult1, TResult2}"/> indicating the outcome of the request, either the user's details or a NotFound result.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Retrieves the current user from the HttpContext.
        /// 2. If the user is found, returns an OK result with the user's details.
        /// 3. If the user is not found, returns a NotFound result.
        /// </remarks>
        [Authorize]
        public static async Task<Results<Ok<UserDto>, NotFound>> Handler(HttpContext ctx, UserManager<ApplicationUser> userManager)
        {
            return await userManager.GetUserAsync(ctx.User)
                is ApplicationUser dbUser ? TypedResults.Ok(new UserDto
                {
                    Name = dbUser.UserName,
                    Balance = dbUser.Balance,
                    Battery = dbUser.Battery,
                    Pro = dbUser.Pro
                }) : TypedResults.NotFound();
        }

        /// <summary>
        /// Handles the request to update the details of the current user.
        /// </summary>
        /// <param name="user">An instance of <see cref="UserForm"/> containing the updated user details.</param>
        /// <param name="ctx">The current <see cref="HttpContext"/> instance.</param>
        /// <param name="userManager">An instance of <see cref="UserManager{TUser}"/> used to manage user-related operations.</param>
        /// <returns>A <see cref="Results{TResult1, TResult2}"/> indicating the outcome of the update operation, either OK or NotFound.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Retrieves the current user from the HttpContext.
        /// 2. If the user is not found, returns a NotFound result.
        /// 3. If the user is found, updates the user's details with the provided values.
        /// 4. Saves the changes and returns an OK result.
        /// </remarks>
        [Authorize]
        public static async Task<Results<Ok, NotFound>> PutHandler(UserForm user, HttpContext ctx, UserManager<ApplicationUser> userManager)
        {
            var dbUser = await userManager.GetUserAsync(ctx.User);

            if (dbUser is null)
                TypedResults.NotFound();


            dbUser.Balance = user.Balance ?? dbUser.Balance;
            dbUser.Pro = user.Pro ?? dbUser.Pro;
            dbUser.Battery = user.Battery ?? dbUser.Battery;

            var result = await userManager.UpdateAsync(dbUser);

            return TypedResults.Ok();
        }
    }
}
