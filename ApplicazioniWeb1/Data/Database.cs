using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApplicazioniWeb1.Data
{
    public class Database : IdentityDbContext<ApplicationUser>
    {
        public Database(DbContextOptions<Database> options) : base(options)
        {

        }

        public DbSet<CarPark> CarParks { get; set; }
        public DbSet<CarSpot> CarSpots { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Book> Books { get; set; }

    }

    public class ApplicationUser : IdentityUser
    {
        public float Balance { get; set; }
        public int Battery { get; set; }
        public bool Pro { get; set; }
    }

    public class CarPark
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public float ParkRate { get; set; }
        public float ChargeRate { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }

        public int Power { get; set; } = 10;

        [JsonIgnore]
        public ICollection<CarSpot> carSpots { get; } = new List<CarSpot>();
    }

    public class CarSpot
    {
        public int Id { get; set; }

        [ForeignKey(nameof(CarPark))]
        public int CarParkId { get; set; }
        public string? UserId { get; set; }
        public DateTime StartLease { get; set; }
        public DateTime EndLease { get; set; }
    }

    [PrimaryKey(nameof(DateStart), nameof(UserId))]
    public class Invoice
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string CarParkId { get; set; }
        public float? Paid { get; set; }
        public float Value { get; set; }
        public string Type { get; set; }
        public float Rate { get; set; }
        public bool Pro { get; set; }
        public float? StartValue { get; set; }
        public float? EndValue { get; set; }
    }

    public class Book
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(CarPark))]
        public int CarParkId { get; set; }
        public DateTime Date { get; set; }
        public float TimeCharge { get; set; }
        public float TimePark { get; set; }
        public bool Entered { get; set; }
        public float TargetCharge { get; set; }
        public float CurrentCharge { get; set; }

    }
}