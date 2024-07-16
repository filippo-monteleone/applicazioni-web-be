using ApplicazioniWeb;
using ApplicazioniWeb1.BackgroundWorkers;
using ApplicazioniWeb1.Data;
using ApplicazioniWeb1.Endpoints;
using ApplicazioniWeb1.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicazioniWeb1.Data.Database>(c =>
    c.UseNpgsql(connectionString: builder.Configuration.GetConnectionString("WebApiDatabase"))
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<QueueWorker>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    if (builder.Environment.IsDevelopment())
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
    Summary = "Login with external provider",
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
    Summary = "Get current logged user",
});

apiEndpoints.MapPut("/user", UserEndpoint.PutHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Edit user",
    Description = "Change user balance, premium status and battery"
}).AddEndpointFilter<EditUserFilter>();

apiEndpoints.MapGet("/role", RoleEndpoint.Handler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Get role of user"
});

apiEndpoints.MapPost("/role", RoleEndpoint.PostHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "User" } },
    Summary = "Set role of user",
}).AddEndpointFilter<SelectRoleFilter>();

apiEndpoints.MapGet("/car-park", CarParkEndpoint.GetHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Get carparks",
});

apiEndpoints.MapPut("/car-park/{id:int}", CarParkEndpoint.PutPark).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Edit a carpark",
}).AddEndpointFilter<EditCarParkFilter>();

apiEndpoints.MapPost("/car-park", CarParkEndpoint.PostHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Create a new carpark",
}).AddEndpointFilter<CreateCarParkFilter>();

apiEndpoints.MapGet("/car-park/{id}/car-spots", CarParkEndpoint.GetCarSpotHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Get carspots of carpark",
});

apiEndpoints.MapDelete("/car-park/{id}", CarParkEndpoint.DeleteHandler).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Delete a carpark",
});

apiEndpoints.MapGet("/car-park/updates", CarParkEndpoint.ParkUpdateSse).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Get carpark notifications",
    Description = "Retrieve the user's position in the queue. The returned value will be either a non-negative integer or -1." +
        "A value of 0 or greater indicates the user's position in the queue." +
        "A value of -1 indicates that the user must now park.",
});

apiEndpoints.MapGet("/car-park/{id}/queue", CarParkEndpoint.GetParkQueue).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Get waiting queue for carpark",
});

apiEndpoints.MapPost("/car-park/{id}/park", CarParkEndpoint.PostPark).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Park user's car",
}).AddEndpointFilter<ParkCarFilter>();

apiEndpoints.MapGet("/car-park/current", CarParkEndpoint.GetCurrentPark).WithOpenApi(o => new(o)
{
    Tags = new List<OpenApiTag> { new() { Name = "Carpark" } },
    Summary = "Get current carpark occupied by user",
});

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