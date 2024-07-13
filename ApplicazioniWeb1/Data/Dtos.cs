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

    public struct PaginatedInvoice
    {
        public IEnumerable<Invoice> invoices { get; set; }
        public int currentPage { get; set; }
        public int pages { get; set; }
        public int length { get; set; }
    }
}
