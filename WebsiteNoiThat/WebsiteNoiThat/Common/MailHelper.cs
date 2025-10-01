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
		public void SendMail(string toEmailAddress, string subject, string content)
		{
			var fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"].ToString();
			var fromEmailDisplayName = ConfigurationManager.AppSettings["FromEmailDisplayName"].ToString();
			var fromEmailPassword = ConfigurationManager.AppSettings["FromEmailPassword"].ToString();
			var smtpHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
			int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTPPort"].ToString());
			bool enabledSsl = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"].ToString());

			string body = content;
			MailMessage message = new MailMessage(new MailAddress(fromEmailAddress, fromEmailDisplayName), new MailAddress(toEmailAddress))
			{
				Subject = subject,
				IsBodyHtml = true,
				Body = body
			};

			var client = new SmtpClient
			{
				EnableSsl = enabledSsl,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword),
				Host = smtpHost,
				Port = smtpPort
			};

			try
			{
				client.Send(message);
			}
			catch (Exception ex)
			{
				// Log the exception or handle it here
				Console.WriteLine("Error sending email: " + ex.Message);
			}
		}
	}
}