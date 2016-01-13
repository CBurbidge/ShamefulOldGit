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
				Console.WriteLine($"Starting Branch info {message.DirPath} -> {message.Branch.Name}");

				var branchLastCommit = message.Branch.Tip;
				var branchInfo = new BranchInfo
				{
					Name = message.Branch.Name,
					CommitterName = branchLastCommit.Committer.Name,
					CommitterEmail = branchLastCommit.Committer.Email,
					Message = branchLastCommit.Message,
					Sha = branchLastCommit.Sha,
				};

				var repoAndBranchInfo = new RepoAndBranchInfo(message.DirPath, branchInfo);

				Context.ActorSelection(ActorSelectionRouting.BranchInfoAggregationActorPath)
					.Tell(new RepositoryAndBranchInfo(repoAndBranchInfo, Sender));
			});
		}

		public class GetBranchInfo
		{
			public GetBranchInfo(string dirPath, Repository repository, Branch branch)
			{
				DirPath = dirPath;
				Repository = repository;
				Branch = branch;
			}

			public string DirPath { get; set; }
			public Repository Repository { get; private set; }
			public Branch Branch { get; private set; }
		}

		public class RepositoryAndBranchInfo
		{
			public RepoAndBranchInfo RepoAndBranchInfo { get; }
			public IActorRef RepositoryActorRef { get; }

			public RepositoryAndBranchInfo(RepoAndBranchInfo repoAndBranchInfo, IActorRef repositoryActorRef)
			{
				RepoAndBranchInfo = repoAndBranchInfo;
				RepositoryActorRef = repositoryActorRef;
			}
		}
	}
}