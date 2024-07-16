
using ApplicazioniWeb1.Endpoints;

namespace ApplicazioniWeb1.Filters
{
    public class EditUserFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var user = context.GetArgument<UserForm>(0);

            if (user.Battery <= 0)
                return TypedResults.Problem("Battery must be > 0", statusCode: 404);

            if (user.Balance <= 0)
                return TypedResults.Problem("Balance must be > 0", statusCode: 404);

            return await next(context);
        }
    }
}
