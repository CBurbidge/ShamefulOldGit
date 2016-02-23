using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Humanizer;

namespace ShamefulOldGit.Actors
{
	public class PrinterActor : ReceiveActor
	{
		public PrinterActor()
		{
			Receive<BranchInfoAggregationActor.BranchInfosToPrint>(message =>
			{
				Logger.WriteLine("Printing out brancheInfos.");
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
		private readonly Func<string, string> h3 = str => $"<h3>{str}</h3>\n";
		private const string _styling = @"<style>
table {
    font-family: ""Trebuchet MS"", Arial, Helvetica, sans-serif;
	table-layout: fixed;
	border-collapse: collapse;
    width: 100%;
}

table td, table th {
    border: 1px solid #ddd;
    text-align: left;
    padding: 8px;
    min-width: 150px;
    max-width: 350px;
}

table tr:nth-child(even)
{
	background-color: #f2f2f2;
}

table tr:hover {
	background-color: #ddd;
}

table th {
	padding-top: 12px;
	padding-bottom: 12px;
	background-color: #4CAF50;
	color: white;
}
</style>
";

		private readonly Func<string, string> htmlMain =
			body => $"<!DOCTYPE html>\n<html>\n<head>\n<title>Git shame</title>\n{_styling}\n</head>\n<body>\n{body}\n</body>\n</html>";

		private readonly Func<string, string> table = str => $"<table>\n{str}\n</table>";
		private readonly Func<string, string> td = str => $"<td>{str}</td>";
		private readonly Func<string, string> tr = str => $"<tr>{str}</tr>\n";
		private readonly Func<string, string> th = str => $"<th>{str}</th>\n";

		public string Build(List<RepoAndBranchInfo> reposAndBranchInfos)
		{
			var sb = new StringBuilder();
			sb.Append($"<p>Here are details of branchs that have not been merged into the branch '{Constants.ComparisonBranchName}' and are older than {Constants.MonthsPriorToNow} months.</p>");
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
						th("Age") +
						th("Branch name") +
						th("Committer's email address.") +
						th("Commit message") +
						th("Commit SHA start")
						)
					);
			var orderedByDateDesc = repoAndBranchInfos.OrderBy(b => b.BranchInfo.CommitterDate.Date);
			foreach (var repoAndBranchInfo in orderedByDateDesc)
			{
				var branchInfo = repoAndBranchInfo.BranchInfo;
				var branchName = branchInfo.Name.Length > Constants.EmailCommitBranchNameLength ? branchInfo.Name.Substring(0, Constants.EmailCommitBranchNameLength) : branchInfo.Name;
				var commitMessage = branchInfo.Message.Length > Constants.EmailCommitMessageLength ? branchInfo.Message.Substring(0, Constants.EmailCommitMessageLength) : branchInfo.Message;
				sb.Append(
					tr(
						td(branchInfo.CommitterDate.Humanize()) +
						td(branchName) +
						td(branchInfo.CommitterEmail) +
						td(commitMessage) +
						td(branchInfo.Sha.Substring(0, Constants.EmailNumberOfCharsOfShaToDisplay))
						)
					);
			}

			return h3(repoName) + table(sb.ToString());
		}
	}
}