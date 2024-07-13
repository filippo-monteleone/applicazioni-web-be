using ApplicazioniWeb1.Endpoints;

namespace ApplicazioniWeb1.Filters
{
    public class EditCarParkFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var form = context.GetArgument<CarParkEndpoint.UpdatePark>(0);

            if (form.ChargeRate < 0) return TypedResults.Problem("Charge rate must be greater or equal 0", statusCode: 400);

            if (form.ParkRate < 0) return TypedResults.Problem("Park rate must be greater or equal 0", statusCode: 400);

            return await next(context);
        }
    }
}
