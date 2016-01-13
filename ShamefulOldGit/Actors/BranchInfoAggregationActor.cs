using System;
using System.Collections.Generic;
using Akka.Actor;
using LibGit2Sharp;

namespace ShamefulOldGit.Actors
{
	public class BranchInfoAggregationActor : ReceiveActor
	{
		private List<RepoAndBranchInfo> _reposAndBranchInfos = new List<RepoAndBranchInfo>();

		public BranchInfoAggregationActor()
		{
			Receive<BranchInfoActor.RepositoryAndBranchInfo>(message =>
			{
				var branchName = message.RepoAndBranchInfo.BranchInfo.Name;
				
				_reposAndBranchInfos.Add(message.RepoAndBranchInfo);

				message.RepositoryActorRef.Tell(new BranchInfoReportedToAggregator(branchName));
			});

			Receive<RepositoriesCoordinatorActor.AllRepositoriesAccountedFor>(message =>
			{
				Context.ActorSelection(ActorSelectionRouting.PrinterActorPath)
					.Tell(new BranchInfosToPrint(_reposAndBranchInfos));
			});

		}

		public class BranchInfosToPrint
		{
			public List<RepoAndBranchInfo> ReposAndBranchInfos { get; set; }

			public BranchInfosToPrint(List<RepoAndBranchInfo> reposAndBranchInfos)
			{
				ReposAndBranchInfos = reposAndBranchInfos;
			}
		}

		public class BranchInfoReportedToAggregator
		{
			public string BranchName { get; private set; }
			
			public BranchInfoReportedToAggregator(string branchName)
			{
				BranchName = branchName;
			}
		}
	}
}