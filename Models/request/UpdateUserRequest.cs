namespace OrderPayment.Models.Request
{
    public class UpdateUserRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; }   // Kullanıcının adı
        public string LastName { get; set; }    // Kullanıcının soyadı
        public string PhoneNumber { get; set; } // Kullanıcının telefon numarası
        public string Password { get; set; }
        public bool IsActive { get; set; }      // Kullanıcının aktiflik durumu
    }
}
