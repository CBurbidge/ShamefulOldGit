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
		private readonly IActorRef _lastEmailedFileActor;

		public EmailingActor(IActorRef lastEmailedFileActor)
		{
			_lastEmailedFileActor = lastEmailedFileActor;
			Receive<PrinterActor.EmailContentToBeSent>(message =>
			{
				
#if DEBUG
				File.WriteAllText(GetPath("Email.html"), message.Content);
#else
				var details = GetEmailDetails();
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

		private EmailDetails GetEmailDetails()
		{
			var emailDetailsFile = File.ReadAllText(GetPath("EmailDetails.txt"));
			var lines = emailDetailsFile.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

			var toLine = CheckStartOfString(lines[0], "to:");
			var fromLine = CheckStartOfString(lines[1], "from:");
			var hostLine = CheckStartOfString(lines[2], "host:");
			var usernameLine = CheckStartOfString(lines[3], "username:");
			var passwordLine = CheckStartOfString(lines[4], "password:");

			return new EmailDetails
			{
				To = toLine,
				From = fromLine,
				Username = usernameLine,
				Host = hostLine,
				Password = passwordLine
			};
		}

		private static string CheckStartOfString(string line, string start)
		{
			if (line.StartsWith(start) == false)
			{
				throw new InvalidProgramException($"Needs to start with '{start}'");
			}
			return line.Replace(start, "");
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

	public class EmailDetails
	{
		public string To { get; set; }
		public string From { get; set; }
		public string Host { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}