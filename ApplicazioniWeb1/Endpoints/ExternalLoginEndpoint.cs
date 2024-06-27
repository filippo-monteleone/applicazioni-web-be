using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Endpoints
{
    public class ExternalLoginEndpoint
    {
        public static IResult Handler(SignInManager<ApplicationUser> signinManager, string provider = "Google")
        {
            var properties = signinManager.ConfigureExternalAuthenticationProperties(provider, "https://localhost:7013/api/confirm-login");
            return Results.Challenge(properties, authenticationSchemes: new List<string>() { provider });
        }
    }
}
