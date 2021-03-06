﻿#define SMTP_YANDEX_RU
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Net;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace BlackHoles
{
  public class MailMessageService : IIdentityMessageService
  {
    public static bool CallbackIsInit { get; private set; }

    static void TryInit()
    {
      if (CallbackIsInit)
        return;

      ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

      CallbackIsInit = true;
    }

    public async Task SendAsync(IdentityMessage message)
    {
      using (var mail = new MailMessage())
      {
        mail.From = new MailAddress("vladdq@yandex.ru");
        mail.To.Add(new MailAddress(message.Destination));
        mail.Subject = message.Subject;
        mail.Body = message.Body;
        mail.IsBodyHtml = true;

        using (var smtp = new SmtpClient("smtp.yandex.ru", 25))
        {
          var email    = ConfigurationManager.AppSettings["EMAIL"];
          var password = ConfigurationManager.AppSettings["MAIL_PASSWORD"];
          smtp.Credentials = new NetworkCredential(email, password);
          smtp.EnableSsl = true;
          await smtp.SendMailAsync(mail);
        }
      }
    } // method

    /// <summary>
    /// Отправка письма на почтовый ящик C# mail send
    /// </summary>
    /// <param name="smtpServer">Имя SMTP-сервера</param>
    /// <param name="from">Адрес отправителя</param>
    /// <param name="password">пароль к почтовому ящику отправителя</param>
    /// <param name="mailto">Адрес получателя</param>
    /// <param name="caption">Тема письма</param>
    /// <param name="message">Сообщение</param>
    /// <param name="attachFile">Присоединенный файл</param>
    public static void SendMail(string mailto, string caption, string message, IEnumerable<string> attachs = null)
    {
      using (var mail = new MailMessage())
      {
        mail.From = new MailAddress(Constants.MainEmail);
        var mailto2 = mailto.Replace("    ", "").Replace("   ", "").Replace("  ", "").Replace(" ", "").Replace(" ", "").Replace("–", "-").Trim(';', ',');

        mail.To.Add(mailto2);
        mail.Subject = caption;
        mail.Body = message;
        mail.IsBodyHtml = true;

        if (attachs != null)
          foreach (var attach in attachs)
          {
            var data = new Attachment(attach, MediaTypeNames.Application.Octet);
            mail.Attachments.Add(data);
          }

        using (var smtp = new SmtpClient("smtp.yandex.ru", 25))
        {
          var email    = ConfigurationManager.AppSettings["EMAIL"];
          var password = ConfigurationManager.AppSettings["MAIL_PASSWORD"];
          smtp.Credentials = new NetworkCredential(email, password);
          smtp.EnableSsl = true;
          smtp.Send(mail);
        }
      }
    } // method
  } // class
} // namespace
