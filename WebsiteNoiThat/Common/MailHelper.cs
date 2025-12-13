using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace WebsiteNoiThat.Common
{
	public class MailHelper
	{

		public bool SendMail(string toEmail, string subject, string body)
		{
			try
			{
				var fromEmail = ConfigurationManager.AppSettings["FromEmailAddress"]
								?? ConfigurationManager.AppSettings["ToEmailAddress"];

				var mail = new MailMessage();
				mail.From = new MailAddress(fromEmail);
				mail.To.Add(toEmail);
				mail.Subject = subject;
				mail.Body = body;
				mail.IsBodyHtml = true;

				var smtp = new SmtpClient();
				smtp.Host = "smtp.gmail.com";
				smtp.Port = 587;
				smtp.UseDefaultCredentials = false;

				smtp.Credentials = new System.Net.NetworkCredential(
					ConfigurationManager.AppSettings["SMTPUser"],
					ConfigurationManager.AppSettings["SMTPPass"]
				);

				smtp.EnableSsl = true;

				smtp.Send(mail);
				return true;
			}
			catch (Exception ex)
			{
				// Bạn có thể log lỗi nếu muốn
				return false;
			}
		}
	}

}