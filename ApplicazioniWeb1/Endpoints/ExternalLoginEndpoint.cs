using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// Handles external login request.
    /// </summary>
    public class ExternalLoginEndpoint
    {
        /// <summary>
        /// Handles external login requests by configuring authentication properties and initiating an authentication challenge.
        /// </summary>
        /// <param name="signinManager"></param>
        /// <param name="provider"></param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the authentication challenge.</returns>
        public static IResult Handler(SignInManager<ApplicationUser> signinManager, string provider = "Google")
        {
            var properties = signinManager.ConfigureExternalAuthenticationProperties(provider, "https://localhost:7013/api/confirm-login");
            return Results.Challenge(properties, authenticationSchemes: new List<string>() { provider });
        }
    }
}
