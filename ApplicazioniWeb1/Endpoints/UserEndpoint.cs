using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ApplicazioniWeb1.Endpoints
{
    public class UserEndpoint
    {
        public class UserForm
        {
            public float? Balance { get; set; }
            public bool? Pro { get; set; }
            public int? Battery { get; set; }
        }

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
