

using ApplicazioniWeb1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static ApplicazioniWeb1.Endpoints.CarParkEndpoint;

namespace ApplicazioniWeb1.BackgroundWorkers
{
    /// <summary>
    /// A background service that processes invoices and car parking entries periodically.
    /// </summary>
    public class QueueWorker : BackgroundService
    {
        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public QueueWorker(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Executes the background service, processing invoices and car parking entries periodically.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token to signal the service to stop.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<Database>();


                var invoices = (from invoice in db.Invoices
                                where invoice.Paid == null && invoice.DateEnd <= DateTime.UtcNow
                                select invoice).ToList();

                foreach (var invoice in invoices)
                {                    
                    if (invoice != null)
                    {
                        invoice.Paid = invoice.Value;

                        var user = (from u in db.Users where u.Id == invoice.UserId select u).Single();

                        user.Balance = (user.Balance - invoice.Paid).Value;
                    }

                    db.SaveChanges();
                }

                var books = (from book in db.Books
                            where book.Entered == false
                            select book).ToList();

                foreach (var book in books)
                {
                    var freeCarSpots = from carspot in db.CarSpots
                                      where carspot.CarParkId == book.CarParkId
                                      && carspot.EndLease < DateTime.UtcNow
                                      select carspot;

                    if (freeCarSpots != null)
                    {
                        var freeCarSpot = freeCarSpots.FirstOrDefault();
                        if (freeCarSpot != null)
                        {
                            freeCarSpot.UserId = book.UserId;
                            freeCarSpot.StartLease = DateTime.UtcNow;



                            var user = db.Users.Where(u => u.Id == book.UserId).FirstOrDefault();
                            var carPark = db.CarParks.Include(c => c.carSpots).SingleOrDefault(carPark => carPark.Id == freeCarSpot.CarParkId);


                            var batteryToCharge = (user.Battery / 100f) * (book.TargetCharge - book.CurrentCharge);
                            var costOfCharge = carPark.ChargeRate * batteryToCharge;
                            var time = batteryToCharge / carPark.Power;

                            freeCarSpot.EndLease = DateTime.UtcNow.AddHours(book.TimePark + time);


                            db.Invoices.AddRange(new List<Invoice>
                            {
                                new Invoice() {
                                    Type = "Charge",
                                    DateStart = DateTime.UtcNow,
                                    DateEnd = DateTime.UtcNow.AddHours(time),
                                    UserId = user.Id,
                                    Rate = carPark.ChargeRate,
                                    Value = costOfCharge,
                                    StartValue = book.CurrentCharge,
                                    EndValue = book.TargetCharge,
                                    CarParkId = carPark.Id.ToString(),
                                    Pro = user.Pro,
                                    OwnerId = carPark.OwnerId
                                },
                                new Invoice() { Type = "Parking",
                                    DateStart = DateTime.UtcNow,
                                    DateEnd = DateTime.UtcNow.AddHours(book.TimePark + time),
                                    UserId = user.Id,
                                    Rate = carPark.ParkRate,
                                    Value = carPark.ParkRate * (book.TimePark + time),
                                    CarParkId = carPark.Id.ToString(),
                                    Pro = user.Pro,
                                    OwnerId = carPark.OwnerId
                                },
                            });

                            book.Entered = true;
                        }

                        db.SaveChanges();
                    }

                }

                Console.WriteLine($"checking queue {books.Count()}");
                await _timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
