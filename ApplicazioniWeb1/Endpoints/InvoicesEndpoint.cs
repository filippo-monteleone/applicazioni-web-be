using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ApplicazioniWeb1.Endpoints.CarParkEndpoint;

namespace ApplicazioniWeb1.Endpoints
{
    public class InvoicesEndpoint
    {

        public class Payment
        {
            public float CostOfCharge { get; set; }
            public float CostOfPark { get; set; }
        }

        public class Filter
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public bool Parking { get; set; }
            public bool Charging { get; set; }
            public bool Basic { get; set; }
            public bool Premium { get; set; }
        }

        public static async Task<IResult> GetHandler(
            DateTime? Start, DateTime? End,
            int page, int resultsPerPage, 
            Database db, UserManager<ApplicationUser> userManager, HttpContext ctx,
            bool Parking = true, bool Charging = true, bool Basic = true,
            bool Premium = true)
        {
            Start = DateTime.MinValue; End = DateTime.MaxValue;
            var user = await userManager.GetUserAsync(ctx.User);
            var carParks = db.CarParks.Where(c => c.OwnerId == user.Id).Select(c => c.Id.ToString());

            var invoices = db.Invoices.Include(i => i.User).Where(i => carParks.Any(c => c == i.CarParkId));

            var pageCount = Math.Ceiling(invoices.Count() / (float)resultsPerPage);

            List<string> parking = new();
            if (Parking)
                parking.Add("Parking");
            if (Charging)
                parking.Add("Charge");

            var paginatedInvoices = await invoices
                .Skip((page - 1) * (int)resultsPerPage)
                .Take((int)resultsPerPage)
                .ToListAsync();



            var temp =  paginatedInvoices.Where(i => {
                if (i.DateStart > Start && i.DateEnd < End)
                {
                    if (Premium && Basic && parking.Contains(i.Type))
                    {
                        return true;
                    } else if (Premium && parking.Contains(i.Type))
                    {
                        return true;
                    } else if (Basic && parking.Contains(i.Type))
                    {
                        return true;
                    }

                }
                return false;
            });

            return Results.Ok(new {
                invoices = temp,
                currentPage = page,
                pages = (int)page,
                length = invoices.Count()
            });

        }

        public  static async Task<IResult> PostCloseHandler(Database db, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            var carSpot = db.CarSpots.FirstOrDefault(c => c.UserId == user.Id && c.EndLease >= DateTime.UtcNow);

            if (carSpot == null)
            {
                return Results.NotFound();
            }

            carSpot.UserId = null;
            carSpot.EndLease = DateTime.UtcNow;

            var carPark = db.CarParks.FirstOrDefault(c => c.Id == carSpot.CarParkId);

            var invoices = db.Invoices.Where(i => i.UserId == user.Id && i.Paid == null);

            foreach (var i in invoices)
            {
                var percentage =  (DateTime.UtcNow - i.DateStart).TotalSeconds / ((i.DateEnd - i.DateStart).TotalSeconds / 100) / 100;
                i.Paid = (float?)((float)i.Value * percentage);
            }

            db.SaveChanges();

            return Results.Ok();
        }
    }
}
