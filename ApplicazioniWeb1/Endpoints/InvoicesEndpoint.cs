using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ApplicazioniWeb1.Endpoints.CarParkEndpoint;

namespace ApplicazioniWeb1.Endpoints
{
    /// <summary>
    /// Endpoint for handling invoice-related operations, such as retrieving, filtering, and closing invoices.
    /// </summary>
    public class InvoicesEndpoint
    {
        /// <summary>
        /// Represents payment details, including costs for charging and parking.
        /// </summary>
        public class Payment
        {
            public float CostOfCharge { get; set; }
            public float CostOfPark { get; set; }
        }

        /// <summary>
        /// Represents filters for querying invoices, including date range and types of invoices.
        /// </summary>
        public class Filter
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public bool Parking { get; set; }
            public bool Charging { get; set; }
            public bool Basic { get; set; }
            public bool Premium { get; set; }
        }

        /// <summary>
        /// Retrieves a paginated list of invoices based on the provided filters.
        /// </summary>
        /// <param name="StartDate">The optional start date for filtering invoices.</param>
        /// <param name="EndDate">The optional end date for filtering invoices.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <param name="resultsPerPage">The number of results per page for pagination.</param>
        /// <param name="db">The database context.</param>
        /// <param name="userManager">The user manager for accessing user information.</param>
        /// <param name="ctx">The HTTP context.</param>
        /// <param name="Parking">Indicates whether to include parking invoices.</param>
        /// <param name="Charging">Indicates whether to include charging invoices.</param>
        /// <param name="Basic">Indicates whether to include basic users.</param>
        /// <param name="Premium">Indicates whether to include premium users.</param>
        /// <returns>A result containing the paginated invoices.</returns>
        [Authorize(Roles = "admin")]
        public static async Task<Ok<PaginatedInvoice>> GetHandler(
            DateTime? StartDate, DateTime? EndDate,
            int page, int resultsPerPage, 
            Database db, UserManager<ApplicationUser> userManager, HttpContext ctx,
            bool Parking = true, bool Charging = true, bool Basic = true,
            bool Premium = true)
        {

            StartDate ??= DateTime.MinValue; EndDate ??= DateTime.MaxValue;
            var user = await userManager.GetUserAsync(ctx.User);
            var carParks = db.CarParks.Where(c => c.OwnerId == user.Id).Select(c => c.Id.ToString());

            var invoices = db.Invoices.Include(i => i.User).Where(i => i.OwnerId == user.Id).ToArray();

            List<string> parking = new();
            if (Parking)
                parking.Add("Parking");
            if (Charging)
                parking.Add("Charge");

            var temp = invoices.Where(i => {
                if (i.DateStart > StartDate && i.DateEnd < EndDate)
                {
                    if (Premium && Basic && parking.Contains(i.Type))
                    {
                        return true;
                    }
                    else if (Premium && parking.Contains(i.Type))
                    {
                        if (i.Pro)
                            return true;
                        else
                            return false;
                    }
                    else if (Basic && parking.Contains(i.Type))
                    {
                        if (!i.Pro) return true;
                        else return false;
                    }
                }
                return false;
            });

            var pageCount = Math.Ceiling(temp.Count() / (float)resultsPerPage);

            var paginatedInvoices = temp
                .Skip((page - 1) * (int)resultsPerPage)
                .Take((int)resultsPerPage);

            return TypedResults.Ok(new PaginatedInvoice {
                invoices = paginatedInvoices,
                currentPage = page,
                pages = (int)page,
                length = temp.Count()
            });

        }

        /// <summary>
        /// Closes active invoices for the current user and updates the car spot information.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="userManager">The user manager for accessing user information.</param>
        /// <param name="ctx">The HTTP context.</param>
        /// <returns>A result indicating whether the operation was successful.</returns>
        [Authorize]
        public  static async Task<Results<NoContent, NotFound>> PostCloseHandler(Database db, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            var carSpot = db.CarSpots.FirstOrDefault(c => c.UserId == user.Id && c.EndLease >= DateTime.UtcNow);

            if (carSpot == null)
            {
                return TypedResults.NotFound();
            }

            carSpot.UserId = null;
            carSpot.EndLease = DateTime.UtcNow;

            var carPark = db.CarParks.FirstOrDefault(c => c.Id == carSpot.CarParkId);

            var invoices = db.Invoices.Where(i => i.UserId == user.Id && i.Paid == null);

            foreach (var i in invoices)
            {
                var percentage =  (DateTime.UtcNow - i.DateStart).TotalSeconds / ((i.DateEnd - i.DateStart).TotalSeconds / 100) / 100;
                i.Paid = (float?)((float)i.Value * percentage);

                user.Balance = (user.Balance - i.Paid).Value;
            }

            db.SaveChanges();

            return TypedResults.NoContent();
        }
    }
}
