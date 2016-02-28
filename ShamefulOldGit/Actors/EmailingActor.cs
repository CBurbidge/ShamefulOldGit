using System;
using System.CodeDom;
using System.IO;
using System.Net;
using System.Net.Mail;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class EmailingActor : ReceiveActor
	{
		private const string EmailSubject = "Old un merged branches";
		private readonly IActorRef _lastEmailedFileActor;

		public EmailingActor(IActorRef lastEmailedFileActor)
		{
			_lastEmailedFileActor = lastEmailedFileActor;
			Receive<PrinterActor.EmailContentToBeSent>(message =>
			{
				
#if DEBUG
				File.WriteAllText("..\\..\\Email.html", message.Content);
#else
				var details = EmailDetails.Get();
				var mail = new MailMessage();
				var client = new SmtpClient();
				client.Port = 587;
				client.Host = details.Host;
				mail.To.Add(new MailAddress(details.To));
				mail.From = new MailAddress(details.From);
				mail.Subject = EmailSubject;
				mail.Body = message.Content;
				mail.IsBodyHtml = true;
				client.EnableSsl = true;
				client.UseDefaultCredentials = false;
				client.Credentials = new NetworkCredential(details.Username, details.Password);
				client.Send(mail);
#endif
				_lastEmailedFileActor.Tell(new EmailedAtTime(DateTime.Now));
			});
		}
		
		public class EmailedAtTime
		{
			public EmailedAtTime(DateTime now)
			{
				Now = now;
			}

			public DateTime Now { get; set; }
		}
	}
}