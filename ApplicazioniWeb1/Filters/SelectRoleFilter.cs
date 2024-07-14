
using ApplicazioniWeb1.Endpoints;

namespace ApplicazioniWeb1.Filters
{
    public class SelectRoleFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var form = context.GetArgument<RoleEndpoint.InviteForm>(0);

            if (form.Invite != "admin" && form.Invite != "user")
            {
                return TypedResults.Problem("Invite not valid", statusCode: 400);
            } 

            return await next(context);
        }
    }
}
