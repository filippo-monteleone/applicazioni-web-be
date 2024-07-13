using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Endpoints
{
    public class LogoutEndpoint
    {
        [Authorize]
        public static async Task<Ok> Handler(SignInManager<ApplicationUser> signInManger)
        {
            await signInManger.SignOutAsync();
            return TypedResults.Ok();
        }
    }
}
