
namespace ApplicazioniWeb1.Filters
{
    public class GetInvoicesFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var startDate = context.GetArgument<DateTime?>(0);
            var endDate = context.GetArgument<DateTime?>(1);

            if (startDate != null)
            {
                if (endDate != null && startDate > endDate)
                {
                    return TypedResults.Problem("EndDate must be > than StartDate", statusCode: 400);
                }
            }

            if (endDate != null)
            {
                if (endDate > DateTime.UtcNow)
                {
                    return TypedResults.Problem("EndDate cannot be from the future", statusCode: 404);
                }
            }


            var page = context.GetArgument<int>(2);
            var resultPerPage = context.GetArgument<int>(3);

            if (page < 0 && resultPerPage < 0)
            {
                return TypedResults.Problem("page and resultsPerPage must be > 0", statusCode: 404);
            }

            return await next(context);
        }
    }
}
