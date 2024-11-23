using OrderPayment.Models;

public class ConfirmOrderViewModel
{
    public Address Address { get; set; }
    public UserCard Card { get; set; }
    public Cart Cart { get; set; }
}
