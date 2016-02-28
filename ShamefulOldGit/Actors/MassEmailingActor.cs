using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class MassEmailingActor : ReceiveActor
	{
		public MassEmailingActor()
		{
			Receive<BranchInfoAggregationActor.BranchInfosToPrint>(message =>
			{
				var details = EmailDetails.Get();
				
				foreach (var reposAndBranchInfo in message.ReposAndBranchInfos)
				{
					var sendingTo = reposAndBranchInfo.BranchInfo.CommitterEmail;
					var subject = GetSubject(reposAndBranchInfo);
					var content = GetContent(reposAndBranchInfo);

					SendEmail(details, sendingTo, subject, content);
				}
			});
		}

		private string GetContent(RepoAndBranchInfo reposAndBranchInfo)
		{
			var emailBuilder = new EmailBuilder();
			return emailBuilder.BuildBranchEmail(reposAndBranchInfo);
		}

		private string GetSubject(RepoAndBranchInfo reposAndBranchInfo)
		{
			return "test";
		}

		private static void SendEmail(EmailDetails details, string sendingTo, string subject, string content)
		{
			var mail = new MailMessage();
			var client = new SmtpClient();
			client.Port = 587;
			client.Host = details.Host;
			mail.To.Add(new MailAddress(sendingTo));
			mail.From = new MailAddress(details.From);
			mail.Subject = subject;
			mail.Body = content;
			mail.IsBodyHtml = true;
			client.EnableSsl = true;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(details.Username, details.Password);
#if DEBUG
			File.WriteAllText("testEmail.txt", content);
#else
			client.Send(mail);
#endif
		}
	}
}