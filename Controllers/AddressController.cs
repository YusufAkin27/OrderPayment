using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderPayment.Models;
using OrderPayment.Models.request;
using OrderPayment.Models.Request;
using System.Linq;

public class AddressController : Controller
{
    private readonly OrderPaymentDbContext _context;

    public AddressController(OrderPaymentDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult AddAddress()
    {
        var model = new CreateAddressRequest();
        return View(model);
    }


    [HttpGet]
    public IActionResult GetAllAddresses()
    {
        var user = GetAuthenticatedUser();
        if (user == null)
        {
            return Unauthorized("Kullanıcı oturum açmamış.");
        }

        var addresses = _context.Address
            .Where(a => a.UserId == user.Id) // Kullanıcıya ait adresleri filtreler
            .Select(a => new AddressViewModel
            {
                Id = a.Id,
                AddressTitle = a.AddressTitle,
                StreetAddress = a.StreetAddress,
                Neighborhood = a.Neighborhood,
                District = a.District,
                City = a.City,
                PhoneNumber = a.PhoneNumber
            })
            .ToList();

        return View(addresses); // Adres listesini View'a gönderir
    }



    [HttpPost]
    public IActionResult AddAddress([FromBody] CreateAddressRequest request)
    {
        // Model doğrulama
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Kullanıcının oturum açıp açmadığını kontrol et
        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");

        // Aynı kullanıcı için var olan bir adres başlığını kontrol et
        var existingAddress = _context.Address
            .FirstOrDefault(a => a.AddressTitle == request.AddressTitle && a.UserId == user.Id);

        if (existingAddress != null)
            return BadRequest("Bu adres başlığı zaten mevcut.");

        // Yeni adresi oluştur
        var newAddress = new Address
        {
            AddressTitle = request.AddressTitle,
            StreetAddress = request.StreetAddress,
            Neighborhood = request.Neighborhood,
            District = request.District,
            City = request.City,
            AddressDescription = request.AddressDescription,
            PhoneNumber = request.PhoneNumber,
            UserId = user.Id,
          
        };

        // Adresi veritabanına ekle
        _context.Address.Add(newAddress);
        _context.SaveChanges();

        return Ok("Adres başarıyla eklendi.");
    }


    // Tüm adresleri getirme


    // Belirli bir adresi ID ile getirme
    [HttpGet]
    public IActionResult GetAddressById([FromQuery] int id)
    {
        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");

        var address = _context.Address
            .FirstOrDefault(a => a.Id == id && a.UserId == user.Id);

        if (address == null)
            return NotFound("Adres bulunamadı veya bu adrese erişim izniniz yok.");

        return Ok(address);
    }

    // Yeni adres ekleme



    // Adres güncelleme
    [HttpPatch]
    public IActionResult UpdateAddress([FromBody] UpdateAddressRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");

        // Kullanıcının sahip olduğu adresi al
        var address = _context.Address.FirstOrDefault(a => a.Id == request.Id && a.UserId == user.Id);
        if (address == null)
            return NotFound("Adres bulunamadı veya bu adrese erişim izniniz yok.");

        // Adres başlığının başka bir kayıtlı adresle çakışıp çakışmadığını kontrol et
        if (!string.IsNullOrEmpty(request.AddressTitle) && request.AddressTitle != address.AddressTitle)
        {
            var existingAddress = _context.Address
                                          .FirstOrDefault(a => a.AddressTitle == request.AddressTitle && a.UserId == user.Id);
            if (existingAddress != null)
            {
                return BadRequest("Bu başlıkla başka bir adresiniz zaten mevcut.");
            }
            // Başlık değişmişse güncelle
            address.AddressTitle = request.AddressTitle;
        }

        // Güncellenen alanları kontrol et ve ata
        if (!string.IsNullOrEmpty(request.StreetAddress) && request.StreetAddress != address.StreetAddress)
            address.StreetAddress = request.StreetAddress;

        if (!string.IsNullOrEmpty(request.Neighborhood) && request.Neighborhood != address.Neighborhood)
            address.Neighborhood = request.Neighborhood;

        if (!string.IsNullOrEmpty(request.District) && request.District != address.District)
            address.District = request.District;

        if (!string.IsNullOrEmpty(request.City) && request.City != address.City)
            address.City = request.City;

        if (!string.IsNullOrEmpty(request.AddressDescription) && request.AddressDescription != address.AddressDescription)
            address.AddressDescription = request.AddressDescription;

        if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != address.PhoneNumber)
            address.PhoneNumber = request.PhoneNumber;

        // Güncellenmiş adresi kaydet
        _context.Address.Update(address);
        _context.SaveChanges();

        return Ok("Adres başarıyla güncellendi.");
    }




    // Adres silme
    [HttpDelete]
    public IActionResult DeleteAddress(int addressId)
    {
        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");

        var address = _context.Address
            .FirstOrDefault(a => a.Id == addressId && a.UserId == user.Id);

        if (address == null)
            return NotFound("Adres bulunamadı veya bu adrese erişim izniniz yok.");

        _context.Address.Remove(address);
        _context.SaveChanges();

        return Ok("Adres başarıyla silindi.");
    }

    // Kullanıcı oturum bilgilerini kontrol et
    private User? GetAuthenticatedUser()
    {
        var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
        if (isAuthenticated != "true")
        {
            HttpContext.Response.Redirect("/Auth/Login");
            return null;
        }

        var userJson = HttpContext.Session.GetString("User");
        return string.IsNullOrEmpty(userJson) ? null : JsonConvert.DeserializeObject<User>(userJson);
    }
}
