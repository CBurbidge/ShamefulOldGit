using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class PrinterActor : ReceiveActor
	{
		public PrinterActor()
		{
			Receive<BranchInfoAggregationActor.BranchInfosToPrint>(message =>
			{
				Console.WriteLine("Printing out brancheInfos.");
				var emailBuilder = new EmailBuilder();

				var emailContent = emailBuilder.Build(message.ReposAndBranchInfos);

				Context.ActorSelection(ActorSelectionRouting.EmailingActorPath)
					.Tell(new EmailContentToBeSent(emailContent));
			});
		}

		public class EmailContentToBeSent
		{
			public EmailContentToBeSent(string content)
			{
				Content = content;
			}

			public string Content { get; }
		}
	}

	public class EmailBuilder
	{
		private const int NumberOfCharsOfShaToDisplay = 6;
		private readonly Func<string, string> h3 = str => $"<h3>{str}</h3>\n";

		private readonly Func<string, string> htmlMain =
			body => $"<!DOCTYPE html>\n<html>\n<head>\n<title>Git shame</title>\n</head>\n<body>\n{body}\n</body>\n</html>";

		private readonly Func<string, string> table = str => $"<table>\n{str}\n</table>";
		private readonly Func<string, string> td = str => $"<td>{str}</td>";
		private readonly Func<string, string> tr = str => $"<tr>{str}</tr>\n";

		public string Build(List<RepoAndBranchInfo> reposAndBranchInfos)
		{
			var sb = new StringBuilder();
			sb.Append($"<p>Here are details of branchs that have not been merged into the branch '{RepositoryActor.ComparisonBranchName}' and are older than {RepositoryActor.MonthsPriorToNow} months.</p>");
			var groupedByRepo = reposAndBranchInfos.GroupBy(r => r.DirPath);
			foreach (var repoAndBranchInfos in groupedByRepo)
			{
				var repoPath = repoAndBranchInfos.Key;
				var repoName = repoPath.Split('\\').Last();
				var repoPrinted = BuildForRepo(repoName, repoAndBranchInfos);
				sb.Append(repoPrinted);
			}
			var reposPartOfEmail = sb.ToString();
			var allHtml = htmlMain(reposPartOfEmail);
			return allHtml;
		}

		private string BuildForRepo(string repoName, IGrouping<string, RepoAndBranchInfo> repoAndBranchInfos)
		{
			var sb = new StringBuilder();
			sb.Append(
					tr(
						td("Branch name") +
						td("Committer's email address.") +
						td("Commit message") +
						td("Commit SHA start")
						)
					);
			foreach (var repoAndBranchInfo in repoAndBranchInfos)
			{
				var branchInfo = repoAndBranchInfo.BranchInfo;
				sb.Append(
					tr(
						td(branchInfo.Name) +
						td(branchInfo.CommitterEmail) +
						td(branchInfo.Message) +
						td(branchInfo.Sha.Substring(0, NumberOfCharsOfShaToDisplay))
						)
					);
			}

			return h3(repoName) + table(sb.ToString());
		}
	}
}