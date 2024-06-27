namespace ApplicazioniWeb
{
    public static class BuildExtension
    {
        public static WebApplication BuildWithSpa(this WebApplicationBuilder builder)
        {
            var app = builder.Build();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(_ => { });

            app.Use((ctx, next) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = 404;
                    return Task.CompletedTask;
                }

                return next();
            });

            app.UseSpa(x => { x.UseProxyToSpaDevelopmentServer("http://localhost:4200/"); });

            return app;
        }
    }
}
