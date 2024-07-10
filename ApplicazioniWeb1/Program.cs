using ApplicazioniWeb;
using ApplicazioniWeb1.BackgroundWorkers;
using ApplicazioniWeb1.Data;
using ApplicazioniWeb1.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicazioniWeb1.Data.Database>(c=> 
    c.UseNpgsql(connectionString: builder.Configuration.GetConnectionString("WebApiDatabase"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<QueueWorker>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    if(builder.Environment.IsDevelopment())
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
    }
}).AddEntityFrameworkStores<ApplicazioniWeb1.Data.Database>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
}).AddCookie().AddGoogle(options =>
{
    options.SignInScheme = IdentityConstants.ExternalScheme;
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    options.Scope.Clear();
    options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");

    options.AccessDeniedPath = "";

    options.Events.OnRemoteFailure = ctx =>
    {
        ctx.Response.Redirect($"/login?error={ctx.Failure?.Message}");
        ctx.HandleResponse();
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

var app = builder.BuildWithSpa();
var apiEndpoints = app.MapGroup("/api");

apiEndpoints.MapGet("/external-login", ExternalLoginEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Authentication" } },
    Summary = "Login with external provider"
});

apiEndpoints.MapGet("/confirm-login", ConfirmLoginEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Authentication" } },
    Summary = "Login with external provider"
});

apiEndpoints.MapPost("/login", LoginEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Authentication" } },
    Summary = "Login with mail and password"
});

apiEndpoints.MapPost("/register", RegisterEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Authentication" } },
    Summary = "Register a new user with mail and password"
});

apiEndpoints.MapGet("/logout", LogoutEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Authentication" } },
    Summary = "Logout the user from the current session"
});

apiEndpoints.MapGet("/user", UserEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Get user"
});

apiEndpoints.MapPut("/user", UserEndpoint.PutHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Edit user"
});

apiEndpoints.MapGet("/role", RoleEndpoint.Handler).WithOpenApi( o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Get role of user"
}).WithTags(new[] { "User" }).WithSummary("Get role of user");

apiEndpoints.MapPost("/role", RoleEndpoint.PostHandler).WithOpenApi(o => new (o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Set role of user",
}); 

apiEndpoints.MapGet("/car-park", CarParkEndpoint.GetHandler).WithTags(new[] { "Carpark" });
apiEndpoints.MapPut("/car-park/{id}", CarParkEndpoint.PutPark).WithTags(new[] { "Carpark" });
apiEndpoints.MapPost("/car-park", CarParkEndpoint.PostHandler).WithTags(new[] { "Carpark" });
apiEndpoints.MapGet("/car-park/{id}/car-spots", CarParkEndpoint.GetCarSpotHandler).WithTags(new[] { "Carpark" });
apiEndpoints.MapDelete("/car-park/{id}", CarParkEndpoint.DeleteHandler).WithTags(new[] { "Carpark" });
apiEndpoints.MapGet("/car-park/updates", CarParkEndpoint.ParkUpdateSse).WithTags(new[] { "Carpark" });
apiEndpoints.MapGet("/car-park/{id}/queue", CarParkEndpoint.GetParkQueue).WithTags(new[] { "Carpark" });
apiEndpoints.MapPost("/car-park/{id}/park", CarParkEndpoint.PostPark).WithTags(new[] { "Carpark" });
apiEndpoints.MapGet("/car-park/current", CarParkEndpoint.GetCurrentPark).WithTags(new[] { "Carpark" });

apiEndpoints.MapGet("/payments", InvoicesEndpoint.GetHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Invoice" } },
    Summary = "Get payments",
});

apiEndpoints.MapPost("/payments/settle", InvoicesEndpoint.PostCloseHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Invoice" } },
    Summary = "Settle a payment",
});

app.Run();