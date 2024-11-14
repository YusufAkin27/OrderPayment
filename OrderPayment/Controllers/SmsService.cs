using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;

public class SmsService
{
    private readonly string _accountSid = "ACedbe8c37a97f5533234623ebc1cf4fad";
    private readonly string _authToken = "480bef62348d8b3c20e7578812223f71";
    private readonly string _fromPhoneNumber = "++16468466310";
    //s
    public SmsService()
    {
        TwilioClient.Init(_accountSid, _authToken);
    }

    public bool SendSms(string toPhoneNumber, string message)
    {
        try
        {
            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(_fromPhoneNumber),
                Body = message
            };

            var messageResource = MessageResource.Create(messageOptions);

            if (messageResource != null && !string.IsNullOrEmpty(messageResource.Sid))
            {
                return true;
            }
            else
            {
                Console.WriteLine($"SMS gönderimi başarısız oldu. Durum: {messageResource?.Status}. Hata mesajı: {messageResource?.ErrorMessage}");
                return false;
            }
        }
        catch (Twilio.Exceptions.ApiException apiEx)
        {
            Console.WriteLine($"Twilio API Hatası: {apiEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Genel hata: {ex.Message}");
            return false;
        }
    }

}
