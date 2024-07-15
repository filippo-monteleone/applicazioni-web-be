
using ApplicazioniWeb1.Data;
using static ApplicazioniWeb1.Endpoints.CarParkEndpoint;

namespace ApplicazioniWeb1.Filters
{
    public class ParkCarFilter : IEndpointFilter
    {
        private Database _db;

        public ParkCarFilter(Database db)
        {
            _db = db;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var id = context.GetArgument<int>(0);

            if (!_db.CarParks.Any(c => c.Id == id))
            {
                return TypedResults.Problem("Id not found", statusCode: 400);
            }

            var park = context.GetArgument<ParkForm>(1);
 
            switch (( park.CurrentCharge, park.TargetCharge))
            {
                case ( < 0, < 0):
                    return TypedResults.Problem("Charges cannot be negative", statusCode: 400);
                case ( > 0, > 0) when park.CurrentCharge > park.TargetCharge:
                    return TypedResults.Problem("Current charge cannot be less than target", statusCode: 400);
                case ( > 0, < 0):
                    return TypedResults.Problem("Target charge cannot be negative", statusCode: 400);
                case ( < 0, > 0):
                    return TypedResults.Problem("Current charge cannot be negative", statusCode: 400);

            }

            if (park.TimePark < 0 || park.TimeCharge < 0)
            {
                return TypedResults.Problem($"Time to {(park.TimePark < 0 ? "park" : "charge")} cannot be negative", statusCode: 400);
            }

            if (park.TimePark + park.TimeCharge == 0)
            {
                return TypedResults.Problem("Total time of parking cannot be equal to 0", statusCode: 400);
            }

            return await next(context);
        }
    }
}
