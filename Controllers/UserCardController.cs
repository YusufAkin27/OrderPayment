using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderPayment.Models;
using OrderPayment.Models.request;
using OrderPayment.Models.Request;
using System.Linq;

public class UserCardController : Controller
{
    private readonly OrderPaymentDbContext _context;

    public UserCardController(OrderPaymentDbContext context)
    {
        _context = context;
    }



    [HttpGet]
    public IActionResult AddCard()
    {
        return View();
    }

  


    [HttpGet]
    public IActionResult GetAllCards()
    {
       
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");
        

        // Kullanıcının kartlarını veritabanından al
        var cards = _context.UserCards
            .Where(c => c.UserId == user.Id) // Sadece oturum açan kullanıcıya ait kartları getir
            .Select(c => new UserCardViewModel
            {
                CardNumber = c.CardNumber.Length >= 4
    ? $"**** **** **** {c.CardNumber.Substring(c.CardNumber.Length - 4)}" // Kart numarasının sadece son 4 hanesi
    : "Invalid card number", // Hatalı kart numarası durumu
                CardHolderName = c.CardHolderName, // Kart sahibi adı
                ExpiryMonth = c.ExpiryMonth, // Son kullanma ayı
                ExpiryYear = c.ExpiryYear, // Son kullanma yılı
                CVV = c.CVV, // CVV bilgisi
                Id = c.Id,
            })
            .ToList();

        // Kartlar bulunmuyorsa mesaj ayarla
        if (!cards.Any())
        {
            ViewBag.Message = "Kullanıcıya ait kayıtlı kart bulunmamaktadır.";
        }

        // Kartları View ile gönder
        return View(cards);
    }



    [HttpPost]
    public IActionResult AddCard([FromBody] CreateUserCardRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");

        // Aynı kartın bu kullanıcıya ait olup olmadığını kontrol et
        var existingCard = _context.UserCards
            .FirstOrDefault(c => c.CardNumber == request.CardNumber && c.UserId == user.Id);

        if (existingCard != null)
            return BadRequest("Bu kart zaten mevcut.");

        // Kartın başka bir kullanıcıya ait olup olmadığını kontrol et
        var cardExistsForAnotherUser = _context.UserCards
            .FirstOrDefault(c => c.CardNumber == request.CardNumber && c.UserId != user.Id);

        if (cardExistsForAnotherUser != null)
            return BadRequest("Bu kart başka bir kullanıcıya ait.");

        // Yeni kartı ekle
        var newCard = new UserCard
        {
            CardNumber = request.CardNumber,
            CardHolderName = request.CardHolderName,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            CVV = request.CVV,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserCards.Add(newCard);
        _context.SaveChanges();

        return Ok("Kart başarıyla eklendi.");
    }
    [HttpGet]
    public IActionResult GetCardDetails([FromQuery] int id)

    {
        try
        {
            // Kullanıcının oturum açıp açmadığını kontrol et
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized(new { message = "Kullanıcı oturum açmamış." });
            }

            // Kullanıcıya ait olan kartı bul
            var card = _context.UserCards
                .FirstOrDefault(c => c.Id == id && c.UserId == user.Id);

            if (card == null)
            {
                return NotFound(new { message = "Kart bulunamadı veya bu karta erişiminiz yok." });
            }

            // Kart bilgilerini DTO olarak döndür
            var cardDetails = new UserCard
            {
                Id = card.Id,
                CardHolderName = card.CardHolderName,
                CardNumber = card.CardNumber,
                ExpiryMonth = card.ExpiryMonth,
                ExpiryYear = card.ExpiryYear,
                CVV = card.CVV
            };

            return Ok(cardDetails);
        }
        catch (Exception ex)
        {
                     
            return StatusCode(500, new { message = "Kart bilgileri alınırken bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
    }



    [HttpPatch]
    public IActionResult UpdateCard([FromBody] UpdateUserCardRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Kullanıcıyı doğrula
        var user = GetAuthenticatedUser();
        if (user == null)
            return Unauthorized("Kullanıcı oturum açmamış.");

        // Kartı veritabanından bul
        var card = _context.UserCards.FirstOrDefault(c => c.Id == request.Id && c.UserId == user.Id);
        if (card == null)
            return NotFound("Kart bulunamadı.");

        // Kullanıcının kendisine ait başka bir kartın numarasını kontrol et
        var cardExistsForThisUser = _context.UserCards
            .FirstOrDefault(c => c.CardNumber == request.CardNumber && c.UserId == user.Id);
        if (cardExistsForThisUser != null && cardExistsForThisUser.Id != card.Id)
        {
            return BadRequest("Bu kart zaten sizin kaydınızda mevcut.");
        }

        // Aynı kart numarasının başka bir kullanıcıya ait olup olmadığını kontrol et
        var cardExistsForAnotherUser = _context.UserCards
            .FirstOrDefault(c => c.CardNumber == request.CardNumber && c.UserId != user.Id);

        if (cardExistsForAnotherUser != null)
            return BadRequest("Bu kart başka bir kullanıcıya ait.");

        // Kart sahibinin adı değiştirildiyse güncelle
        if (!string.IsNullOrEmpty(request.CardHolderName) && request.CardHolderName != card.CardHolderName)
        {
            card.CardHolderName = request.CardHolderName;
        }

        // Son kullanma ayı değiştirildiyse güncelle
        if (request.ExpiryMonth.HasValue && request.ExpiryMonth != card.ExpiryMonth)
        {
            card.ExpiryMonth = request.ExpiryMonth.Value;
        }

        // Son kullanma yılı değiştirildiyse güncelle
        if (request.ExpiryYear.HasValue && request.ExpiryYear != card.ExpiryYear)
        {
            card.ExpiryYear = request.ExpiryYear.Value;
        }

        // CVC kodu değiştirildiyse güncelle
        if (!string.IsNullOrEmpty(request.CVV) && request.CVV != card.CVV)
        {
            card.CVV = request.CVV;
        }

        _context.UserCards.Update(card);
        _context.SaveChanges();

        return Ok("Kart başarıyla güncellendi.");
    }




    [HttpDelete]
    public IActionResult DeleteCard(int cardId)
    {
        // Kullanıcıyı doğrula
        var user = GetAuthenticatedUser();
        if (user == null)
        {
            return Unauthorized("Kullanıcı oturum açmamış.");
        }

        // Kartı kullanıcıya ait olup olmadığını kontrol et
        var card = _context.UserCards
                            .Where(c => c.Id == cardId && c.UserId == user.Id)
                            .FirstOrDefault();

        if (card == null)
        {
            // Kart bulunamadı veya kullanıcı bu karta erişim iznine sahip değil
            return NotFound("Kart bulunamadı veya bu karta erişim izniniz yok.");
        }

        // (Opsiyonel) Kart üzerinde ek kontroller yapılabilir
        // Örneğin, kartın durumunu (aktif mi pasif mi) kontrol etmek gibi

        // Kartı sil
        try
        {
            _context.UserCards.Remove(card);
            _context.SaveChanges();
            return Ok("Kart başarıyla silindi.");
        }
        catch (Exception ex)
        {
            // Hata durumunda
            return StatusCode(500, $"Kart silinirken bir hata oluştu: {ex.Message}");
        }
    }


    // Kullanıcının oturum bilgilerini kontrol et
    private User? GetAuthenticatedUser()
    {
        var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
        if (isAuthenticated != "true")
        {
            // Kullanıcı doğrulama başarısızsa login sayfasına yönlendir
            HttpContext.Response.Redirect("/Auth/Login");
            return null;
        }

        var userJson = HttpContext.Session.GetString("User");
        return string.IsNullOrEmpty(userJson) ? null : JsonConvert.DeserializeObject<User>(userJson);
    }

}
