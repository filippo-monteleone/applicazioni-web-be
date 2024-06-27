using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicazioniWeb1.Endpoints
{
    public class LoginEndpoint
    {
        public class LoginForm
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public static async Task<IResult> Handler([FromBody] LoginForm login, SignInManager<ApplicationUser> signInManager)
        {
            var result = await signInManager.PasswordSignInAsync(login.Username, login.Password, true, false);

            if (result.Succeeded)
            {
                return Results.Ok();
            }

            return Results.BadRequest();
        }
    }
}
