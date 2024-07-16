using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApplicazioniWeb1.Data
{
    public struct UserDto
    {
        public string Name { get; set; }
        public float Balance { get; set; }
        public bool Pro { get; set; }
        public int Battery { get; set; }
    }

    public struct CarParkDto
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public float ParkRate { get; set; }
        public float ChargeRate { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public int Power { get; set; }
    }

    public struct QueueInfo
    {
        public int Queue { get; set; }
    }

    public struct ParkInfo
    {
        public enum Info
        {
            Free,
            Full
        }
        public Info Status { get; set; }
        public DateTime? EndParking { get; set; }
    }


    //Id = carParkReserved.Id,
    //                Name = carParkReserved.Name,
    //                ParkRate = carParkReserved.ParkRate,
    //                ChargeRate = carParkReserved.ChargeRate,
    //                InQueue = true,
    //                pos = list.ToList().FindIndex(b => b.UserId == user.Id)


    //Id = carPark.Id,
    //            Name = carPark.Name,
    //            ParkRate = carPark.ParkRate,
    //            ChargeRate = carPark.ChargeRate,
    //            InQueue = false,
    //            EndParking = carSpot.EndLease,
    //            chargeCurrent = toPay + park,
    //            stepCurrent = step,
    //            stepPark = parkStep,
    //            batteryStep,
    //            battery
    public struct CurrentPark
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float ParkRate { get; set; }
        public float ChargeRate { get; set; }
        public bool InQueue { get; set; }
        public int? Pos { get; set; }
        public float? ChargeCurrent { get; set; }
        public double StepCurrent { get; set; }
        public double StepPark { get; set; }
        public double BatteryStep { get; set; }
        public float? Battery { get; set; }
        public DateTime EndParking { get; set; }
    }

    public struct PaginatedInvoice
    {
        public IEnumerable<Invoice> invoices { get; set; }
        public int currentPage { get; set; }
        public int pages { get; set; }
        public int length { get; set; }
    }

    public struct PaginatedCarSpots
    {
        public IEnumerable<CarSpot> carSpots { get; set; }
        public int currentPage { get; set; }
        public int pages { get; set; }
        public int length { get; set; }
    }
}
