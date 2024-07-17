
using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;

namespace ApplicazioniWeb1.Filters
{
    public class GetCarSpotsFilter : IEndpointFilter
    {
        private Database _db;
        public GetCarSpotsFilter(Database db)
        {
            _db = db;
        }
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var page = context.GetArgument<int>(0);
            var resultPerPage = context.GetArgument<int>(1);

            if (page < 0 && resultPerPage < 0)
            {
                return TypedResults.Problem("page and resultsPerPage must be > 0", statusCode: 404);
            }

            return await next(context);
        }
    }
}
