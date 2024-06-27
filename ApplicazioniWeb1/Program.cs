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
using System.Net.Http.Headers;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicazioniWeb1.Data.Database>(c=> 
    c.UseNpgsql(connectionString: builder.Configuration.GetConnectionString("WebApiDatabase"))
);

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

apiEndpoints.MapGet("/external-login", ExternalLoginEndpoint.Handler);
apiEndpoints.MapGet("/confirm-login", ConfirmLoginEndpoint.Handler);

apiEndpoints.MapPost("/login", LoginEndpoint.Handler);
apiEndpoints.MapPost("/register", RegisterEndpoint.Handler);
apiEndpoints.MapGet("/logout", LogoutEndpoint.Handler);

apiEndpoints.MapGet("/user", UserEndpoint.Handler);
apiEndpoints.MapPut("/user", UserEndpoint.PutHandler);

apiEndpoints.MapGet("/role", RoleEndpoint.Handler);
apiEndpoints.MapPost("/role", RoleEndpoint.PostHandler);

apiEndpoints.MapGet("/car-park", CarParkEndpoint.GetHandler);
apiEndpoints.MapPost("/car-park", CarParkEndpoint.PostHandler);
apiEndpoints.MapGet("/car-park/{id}/car-spots", CarParkEndpoint.GetCarSpotHandler);
apiEndpoints.MapDelete("/car-park/{id}", CarParkEndpoint.DeleteHandler);
apiEndpoints.MapGet("/car-park/updates", CarParkEndpoint.ParkUpdateSse);

apiEndpoints.MapPost("/car-park/{id}/park", CarParkEndpoint.PostPark);
apiEndpoints.MapGet("/car-park/current", CarParkEndpoint.GetCurrentPark);

apiEndpoints.MapGet("/payments", InvoicesEndpoint.GetHandler);
apiEndpoints.MapPost("/payments/settle", InvoicesEndpoint.PostCloseHandler);

app.Run();