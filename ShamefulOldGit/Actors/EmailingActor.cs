using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class EmailingActor : ReceiveActor
	{
		private const string EmailSubject = "Old un merged branches";
		private const string EmailToAddress = "chester.burbidge@abide-financial.com";
		private const string EmailFromAddress = "chester.burbidge@abide-financial.com";
		private readonly IActorRef _lastEmailedFileActor;

		public EmailingActor(IActorRef lastEmailedFileActor)
		{
			_lastEmailedFileActor = lastEmailedFileActor;
			Receive<PrinterActor.EmailContentToBeSent>(message =>
			{
				var mail = new MailMessage();
				var client = new SmtpClient();
				var username = File.ReadAllText(GetPath("Username.txt"));
				var password = File.ReadAllText(GetPath("Password.txt"));
				client.Credentials = new NetworkCredential(username, password);
				client.Port = 25;
				client.Host = File.ReadAllText(GetPath("Host.txt"));
				mail.To.Add(new MailAddress(EmailToAddress));
				mail.From = new MailAddress(EmailFromAddress);
				mail.Subject = EmailSubject;
				mail.Body = message.Content;
				client.Send(mail);

				_lastEmailedFileActor.Tell(new EmailedAtTime(DateTime.Now));
			});
		}

		private string GetPath(string path)
		{
			return Path.GetFullPath($"..\\..\\..\\{path}");
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