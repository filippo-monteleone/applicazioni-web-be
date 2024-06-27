

using ApplicazioniWeb1.Data;

namespace ApplicazioniWeb1.BackgroundWorkers
{
    public class QueueWorker : BackgroundService
    {
        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public QueueWorker(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<Database>();

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
                            freeCarSpot.EndLease = DateTime.UtcNow.AddHours(book.TimeSpan);
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
