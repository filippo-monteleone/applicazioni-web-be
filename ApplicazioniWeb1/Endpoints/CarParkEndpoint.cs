using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Timers;

namespace ApplicazioniWeb1.Endpoints
{
    public class CarParkEndpoint
    {
        public class CarPark
        {
            public string Name { get; set; }
            public int CarSpots { get; set; }
            public float ParkRate { get; set; }
            public float ChargeRate { get; set; }
            public string Lat { get; set; }
            public string Lng { get; set; }
            public int Power { get; set; }
        }

        public class ParkForm
        {
            public float CurrentCharge { get; set; }
            public float TargetCharge { get; set; }
            public float TimeCharge { get; set; }
            public float TimePark { get; set; }
        }

        [Authorize(Roles = "admin")]
        public static async Task<IResult> PostHandler(CarPark carPark, Database db, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            var c = db.CarParks.Add(new Data.CarPark {
                Name = carPark.Name,
                ChargeRate = carPark.ChargeRate,
                ParkRate = carPark.ParkRate,
                Lat = carPark.Lat,
                Long = carPark.Lng,
                OwnerId = user.Id,
                Power = carPark.Power
            });

            db.SaveChanges();

            if (carPark.CarSpots <= 0)
            {
                return Results.BadRequest();
            }

            for (int i = 0; i < carPark.CarSpots; i++)
                db.CarSpots.Add(new CarSpot { CarParkId = c.Entity.Id });

            db.SaveChanges();


            return Results.Ok();
        }

        public static async Task<IResult> GetHandler(Database db, UserManager<ApplicationUser> userManager, HttpContext ctx, [FromQuery] bool me = false) {
            var user = await userManager.GetUserAsync(ctx.User);

            var carParks = db.CarParks.Where(carPark => me ? carPark.OwnerId == user.Id : true);

            return Results.Ok(carParks);

        }

        [Authorize(Roles = "admin")]
        public static async Task<IResult> DeleteHandler(Database db, int id, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {

            var user = await userManager.GetUserAsync(ctx.User);

            var carPark = db.CarParks.SingleOrDefault(c => c.Id == id && c.OwnerId == user.Id);

            if (carPark != null)
            {
                db.CarParks.Remove(carPark);
                db.SaveChanges();

                return Results.Ok();
            }

            return Results.NotFound();
        }

        [Authorize(Roles = "admin")]
        public static async Task<IResult> GetCarSpotHandler(int id, [FromQuery] int page, [FromQuery] int resultsPerPage, Database db, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            var carPark = db.CarParks.Where(carPark => carPark.Id == id && carPark.OwnerId == user.Id).SingleOrDefault();

            if (carPark == null || page <= 0)
                return Results.NotFound();

            var carSpots = (db.CarSpots.Where(carSpot => carSpot.CarParkId == carPark.Id)).ToList();

            var pageCount = Math.Ceiling(carSpots.Count() / (float)resultsPerPage);

            var paginatedCarSpots = carSpots
                .Skip((page - 1) * (int)resultsPerPage)
                .Take((int)resultsPerPage);

            foreach (var spot in paginatedCarSpots)
            {
                if (spot.EndLease < DateTime.UtcNow) spot.UserId = null;
            }

            return Results.Ok(new {
                carSpots = paginatedCarSpots,
                currentPage = page,
                pages = (int)page,
                length = carSpots.Count()
            });
        }

        public static async Task<IResult> PostPark(int id, ParkForm parkForm, Database db, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var carPark = db.CarParks.Include(c => c.carSpots).SingleOrDefault(carPark => carPark.Id == id);

            if (carPark == null)
                return Results.Problem();

            var user = await userManager.GetUserAsync(ctx.User);
            if (user.Battery == 0)
                return Results.Problem();


            var freeSpot = carPark.carSpots.FirstOrDefault(s => s.EndLease < DateTime.UtcNow);

            if (freeSpot == null)
            {
                db.Books.Add(new Book
                {
                    UserId = user.Id,
                    Date = DateTime.UtcNow,
                    CarParkId = carPark.Id,
                    Entered = false,
                    TimeCharge = parkForm.TimeCharge,
                    TimePark = parkForm.TimePark,
                    TargetCharge = parkForm.TargetCharge,
                    CurrentCharge = parkForm.CurrentCharge,
                });
                db.SaveChanges();
                return Results.Ok(new
                {
                    Status = "Full"
                });
            }

            var batteryToCharge = (user.Battery / 100f) * (parkForm.TargetCharge - parkForm.CurrentCharge);
            var costOfCharge = carPark.ChargeRate * batteryToCharge;
            var time = batteryToCharge / carPark.Power;

            db.Invoices.AddRange(new List<Invoice>
                {
                    new Invoice() {
                        Type = "Charge",
                        DateStart = DateTime.UtcNow,
                        DateEnd = DateTime.UtcNow.AddHours(time),
                        UserId = user.Id,
                        Rate = carPark.ChargeRate,
                        Value = costOfCharge,
                        StartValue = parkForm.CurrentCharge,
                        EndValue = parkForm.TargetCharge,
                        CarParkId = id.ToString(),
                        Pro = user.Pro
                    },
                    new Invoice() { Type = "Parking",
                        DateStart = DateTime.UtcNow,
                        DateEnd = DateTime.UtcNow.AddHours(parkForm.TimePark + time),
                        UserId = user.Id,
                        Rate = carPark.ParkRate,
                        Value = carPark.ParkRate * parkForm.TimePark,
                        CarParkId = id.ToString(),
                        Pro = user.Pro
                    },
                }) ;

            db.SaveChanges();

            freeSpot.UserId = user.Id;
            freeSpot.StartLease = DateTime.UtcNow;
            freeSpot.EndLease = DateTime.UtcNow.AddHours(parkForm.TimePark + time);

            db.SaveChanges();

            return Results.Ok(new
            {
                Status = "Free",
                EndParking = freeSpot.EndLease
            }) ;
        }

        public static async Task ParkUpdateSse(HttpContext ctx, Database db, UserManager<ApplicationUser> userManager, CancellationToken token)
        {
            ctx.Response.Headers.Add("Content-Type", "text/event-stream");
            var waitingUsers = new Dictionary<string, int>();


            while(!token.IsCancellationRequested)
            {

                var user = await userManager.GetUserAsync(ctx.User);

                var myReservation = db.Books.FirstOrDefault(b => b.UserId == user.Id && !b.Entered);

                if (myReservation != null)
                {
                    var list = from book in db.Books
                               where myReservation.CarParkId == book.CarParkId && book.Entered == false
                               orderby book.Date
                               select book;

                    var pos = list.ToList().FindIndex(b => b.UserId == user.Id);

                    if (waitingUsers.TryAdd(user.Id, pos))
                    {
                        var text = JsonSerializer.Serialize(new {pos});

                        await ctx.Response.WriteAsync($"data: {text}\n\n");
                        await ctx.Response.Body.FlushAsync();

                    }
                }
                else if (waitingUsers.ContainsKey(user.Id))
                {

                    var carSpot = db.CarSpots.Where(c => c.UserId == user.Id && c.EndLease > DateTime.UtcNow).Single();

                    var time = (carSpot.EndLease - carSpot.StartLease).TotalHours;

                    var text = JsonSerializer.Serialize(new { pos = -1, time });


                    await ctx.Response.WriteAsync($"data: {text}\n\n");

                    await ctx.Response.Body.FlushAsync();

                    waitingUsers.Remove(user.Id);
                }

                await Task.Delay(1000);
            }

            //while (true)
            //{
            //    var user = await userManager.GetUserAsync(ctx.User);

            //    var myReservation = db.Books.FirstOrDefault(b => b.UserId == user.Id && !b.Entered);

            //    if (myReservation != null)
            //    {
            //        var list = from book in db.Books
            //                   where myReservation.CarParkId == book.CarParkId && book.Entered == false
            //                   orderby book.Date 
            //                   select book;

            //        await ctx.Response.WriteAsync($"data: {list.ToList().FindIndex(b => b.UserId == user.Id)}\n\n");

            //        await ctx.Response.Body.FlushAsync();
            //        await Task.Delay(1000);
            //    } else
            //    {
            //        await ctx.Response.WriteAsync($"data: {-1}\n\n");

            //        await ctx.Response.Body.FlushAsync();
            //        await Task.Delay(1000);
            //    }

            //}
        }

        public static async Task<IResult> GetCurrentPark(Database db, UserManager<ApplicationUser> userManager, HttpContext ctx)
        {
            var user = await userManager.GetUserAsync(ctx.User);

            var carSpot = db.CarSpots.FirstOrDefault(c => c.UserId == user.Id && c.EndLease >= DateTime.UtcNow);
            var myReservation = db.Books.FirstOrDefault(b => b.UserId == user.Id && !b.Entered);

            if (myReservation != null)
            {
                var list = from book in db.Books
                           where myReservation.CarParkId == book.CarParkId && book.Entered == false
                           orderby book.Date
                           select book;

                var carParkReserved = db.CarParks.FirstOrDefault(c => c.Id == myReservation.CarParkId);

                return Results.Ok(new { 
                    Id = carParkReserved.Id,
                    Name = carParkReserved.Name,
                    ParkRate = carParkReserved.ParkRate,
                    ChargeRate = carParkReserved.ChargeRate,
                    InQueue = true,
                    pos = list.ToList().FindIndex(b => b.UserId == user.Id) 
                });
            }

            if (carSpot == null)
            {

                return Results.NotFound();
            }

            var carPark = db.CarParks.FirstOrDefault(c => c.Id == carSpot.CarParkId);

            var invoices = from invoice in db.Invoices
                           where invoice.Paid == null && invoice.UserId == user.Id
                           select invoice;

            float? toPay = 0, park = 0, battery = 0;
            double step = 0, parkStep = 0, batteryStep = 0;
            

            foreach (var i in invoices)
            {
                if (i.Type == "Charge")
                {
                    var percentage = (DateTime.UtcNow - i.DateStart).TotalSeconds / ((i.DateEnd - i.DateStart).TotalSeconds / 100) / 100;
                    toPay = (float?)((float)i.Value * percentage);
                    step = i.Value / (i.DateEnd - i.DateStart).TotalSeconds;
                    batteryStep = (double)(i.EndValue - i.StartValue) / (i.DateEnd - i.DateStart).TotalSeconds;
                    var charged = batteryStep * (DateTime.UtcNow - i.DateStart).TotalSeconds;
                    battery = (float)charged + i.StartValue;
                }
                else if (i.Type == "Parking")
                {
                    var percentage = (DateTime.UtcNow - i.DateStart).TotalSeconds / ((i.DateEnd - i.DateStart).TotalSeconds / 100) / 100;
                    park = (float?)((float)i.Value * percentage);
                    parkStep = i.Value / (i.DateEnd - i.DateStart).TotalSeconds;
                }
            }

            return Results.Ok(new
            {
                Id = carPark.Id,
                Name = carPark.Name,
                ParkRate = carPark.ParkRate,
                ChargeRate = carPark.ChargeRate,
                InQueue = false,
                EndParking = carSpot.EndLease,
                chargeCurrent = toPay + park,
                stepCurrent = step,
                stepPark = parkStep,
                batteryStep,
                battery
            }) ;
        }
    }
}
