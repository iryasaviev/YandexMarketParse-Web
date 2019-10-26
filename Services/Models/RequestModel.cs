namespace Services.Models
{
    public class RequestModel
    {
        public string Text { get; set; }
        public string PartNumber { get; set; }
        public string City { get; set; }
        public int PageCount { get; set; }
        public int PositionsCount { get; set; }
    }
}