namespace ApplicazioniWeb1.Data
{
    public struct UserDto
    {
        public string Name { get; set; }
        public float Balance { get; set; }
        public bool Pro { get; set; }
        public int Battery { get; set; }
    }

    public struct PaginatedInvoice
    {
        public IEnumerable<Invoice> invoices { get; set; }
        public int currentPage { get; set; }
        public int pages { get; set; }
        public int length { get; set; }
    }
}
