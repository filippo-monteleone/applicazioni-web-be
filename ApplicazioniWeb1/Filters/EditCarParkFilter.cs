using ApplicazioniWeb1.Data;
using ApplicazioniWeb1.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Filters
{
    public class EditCarParkFilter : IEndpointFilter
    {

        private Database _db;

        public EditCarParkFilter(Database db)
        {
            _db = db;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {

            var id = context.GetArgument<int>(0);

            var carpark = _db.CarParks.Any(c => c.Id == id);

            if (!carpark) return TypedResults.Problem("Id not found", statusCode: 404);

            var form = context.GetArgument<CarParkEndpoint.UpdatePark>(1);

            if (form.ChargeRate < 0) return TypedResults.Problem("Charge rate must be greater or equal 0", statusCode: 400);

            if (form.ParkRate < 0) return TypedResults.Problem("Park rate must be greater or equal 0", statusCode: 400);

            return await next(context);
        }
    }
}
