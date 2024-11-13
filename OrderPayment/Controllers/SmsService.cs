using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;

public class SmsService
{
    private readonly string _accountSid = "AC8cae01f4e341bfccf50cf361b381231d";
    private readonly string _authToken = "8a73fd1102f994f81d79d46ebd44d709";
    private readonly string _fromPhoneNumber = "+19198137836";
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
