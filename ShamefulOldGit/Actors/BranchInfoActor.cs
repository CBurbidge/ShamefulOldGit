using System;
using Akka.Actor;
using LibGit2Sharp;

namespace ShamefulOldGit.Actors
{
	public class BranchInfoActor : ReceiveActor
	{
		public BranchInfoActor()
		{
			Receive<GetBranchInfo>(message =>
			{
				Logger.WriteLine($"Starting Branch info {message.DirPath} -> {message.BranchName}");
				using (var repository = new Repository(message.RepositoryPath))
				{
					var branch = repository.Branches[message.BranchName];
					var branchLastCommit = branch.Tip;
					var branchInfo = new BranchInfo
					{
						Name = branch.Name,
						CommitterDate = branchLastCommit.Committer.When,
						CommitterName = branchLastCommit.Committer.Name,
						CommitterEmail = branchLastCommit.Committer.Email,
						Message = branchLastCommit.Message,
						Sha = branchLastCommit.Sha,
					};

					var repoAndBranchInfo = new RepoAndBranchInfo(message.DirPath, branchInfo);

					Context.ActorSelection(ActorSelectionRouting.BranchInfoAggregationActorPath)
						.Tell(new RepositoryAndBranchInfo(repoAndBranchInfo));

				}
			});
		}

		public class GetBranchInfo
		{
			public GetBranchInfo(string dirPath, string repositoryPath, string branchName)
			{
				DirPath = dirPath;
				RepositoryPath = repositoryPath;
				BranchName = branchName;
			}

			public string DirPath { get; set; }
			public string RepositoryPath { get; private set; }
			public string BranchName { get; private set; }
		}

		public class RepositoryAndBranchInfo
		{
			public RepoAndBranchInfo RepoAndBranchInfo { get; }

			public RepositoryAndBranchInfo(RepoAndBranchInfo repoAndBranchInfo)
			{
				RepoAndBranchInfo = repoAndBranchInfo;
			}
		}
	}
}