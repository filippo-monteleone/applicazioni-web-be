using ApplicazioniWeb1.Data;
using ApplicazioniWeb1.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApplicazioniWeb1.Filters
{
    public class CreateCarParkFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var form = context.GetArgument<CarParkEndpoint.CarPark>(0);

            if (form.CarSpots <= 0) return TypedResults.Problem("Car spots must be greater than 0", statusCode: 400);

            if (form.ChargeRate < 0) return TypedResults.Problem("Charge rate must be greater or equal 0", statusCode: 400);

            if (form.ParkRate < 0) return TypedResults.Problem("Park rate must be greater or equal 0", statusCode: 400);

            if (form.Power <= 0) return TypedResults.Problem("Power erogated must be greater than 0", statusCode: 400);

            return await next(context);
        }
    }
}
