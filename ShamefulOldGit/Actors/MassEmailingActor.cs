using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class MassEmailingActor : ReceiveActor
	{
		private readonly EmailBuilder _emailBuilder = new EmailBuilder();
		
		public MassEmailingActor()
		{
			Receive<BranchInfoAggregationActor.BranchInfosToPrint>(message =>
			{
				var details = EmailDetails.Get();
				
				foreach (var reposAndBranchInfo in message.ReposAndBranchInfos)
				{
#if DEBUG
					string sendingTo = "fake@email.com";
#else
					var committerEmail = reposAndBranchInfo.BranchInfo.CommitterEmail;
					var sendingTo = details.Exceptions.ContainsKey(committerEmail) 
						? details.Exceptions[committerEmail] 
						: committerEmail ;
#endif
					var subject = GetSubject(reposAndBranchInfo);
					var content = GetContent(reposAndBranchInfo);

					try
					{
						SendEmail(details, sendingTo, subject, content);
					}
					catch (Exception e)
					{
						Logger.WriteLine($"Exception happened - {e.Message}");
						Logger.SaveToFile();
					}
				}
			});
		}

		private string GetContent(RepoAndBranchInfo reposAndBranchInfo)
		{
			return _emailBuilder.BuildBranchEmail(reposAndBranchInfo);
		}

		private string GetSubject(RepoAndBranchInfo reposAndBranchInfo)
		{
			var repoName = reposAndBranchInfo.DirPath.Split('\\').Last();

			return $"{repoName} - {reposAndBranchInfo.BranchInfo.Name} - possibly stale";
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

			if (Constants.SendEmails)
			{
				client.Send(mail);
			}
			else
			{
				Logger.WriteLine("SENDING MASS EMAIS IS SET TO FALSE");
				Logger.SaveToFile();
				File.WriteAllText("testEmail.html", content);
			}
		}
	}
}