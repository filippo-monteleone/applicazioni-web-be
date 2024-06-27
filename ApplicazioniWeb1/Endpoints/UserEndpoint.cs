using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
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
        public static async Task<IResult> Handler(HttpContext ctx, UserManager<ApplicationUser> userManager)
        {
            var user = ctx.User;

            var dbUser = await userManager.GetUserAsync(user);

            return Results.Ok(new { 
                Name = dbUser.UserName, 
                Balance = dbUser.Balance, 
                Battery = dbUser.Battery, 
                Pro = dbUser.Pro 
            }) ;
        }

        public static async Task<IResult> PutHandler(UserForm user, HttpContext ctx, UserManager<ApplicationUser> userManager)
        {
            var dbUser = await userManager.GetUserAsync(ctx.User);

            dbUser.Balance = user.Balance ?? dbUser.Balance;
            dbUser.Pro = user.Pro ?? dbUser.Pro;
            dbUser.Battery = user.Battery ?? dbUser.Battery;

            var result = await userManager.UpdateAsync(dbUser);

            return Results.Ok(result);
        }
    }
}
