namespace OrderPayment.Models.request
{
    public class AddressViewModel
    {
        public int Id { get; set; }
        public string AddressTitle { get; set; }
        public string StreetAddress { get; set; }
        public string Neighborhood { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
    }
}
