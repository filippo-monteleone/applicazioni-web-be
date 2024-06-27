using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Endpoints
{
    public class LogoutEndpoint
    {
        public static async Task<IResult> Handler(SignInManager<ApplicationUser> signInManger)
        {
            await signInManger.SignOutAsync();
            return Results.Ok();
        }
    }
}
