using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class BranchInfoAggregationActor : ReceiveActor
	{
		private List<RepoAndBranchInfo> _reposAndBranchInfos = new List<RepoAndBranchInfo>();

		public BranchInfoAggregationActor()
		{
			Receive<BranchInfoActor.RepositoryAndBranchInfo>(message =>
			{
				var containsBranchAlready = _reposAndBranchInfos
					.Any(
						r => r.BranchInfo.Name == message.RepoAndBranchInfo.BranchInfo.Name
						&& r.DirPath == message.RepoAndBranchInfo.DirPath);

				if (containsBranchAlready == false)
				{
					_reposAndBranchInfos.Add(message.RepoAndBranchInfo);
				}

				Context.ActorSelection(ActorSelectionRouting.RepositoriesCoordinatorActorPath)
					.Tell(new BranchInfoReportedToAggregator(message.RepoAndBranchInfo));
			});

			Receive<Clear>(message =>
			{
				_reposAndBranchInfos.Clear();
			});

			Receive<RepositoriesCoordinatorActor.AllRepositoriesAccountedFor>(message =>
			{
				if (_reposAndBranchInfos.Count > 0)
				{
					Context.ActorSelection(ActorSelectionRouting.PrinterActorPath)
						.Tell(new BranchInfosToPrint(_reposAndBranchInfos));

					Context.ActorSelection(ActorSelectionRouting.MassEmailingActorPath)
						.Tell(new BranchInfosToPrint(_reposAndBranchInfos));
				}
				else
				{
					Logger.WriteLine("There are no branches to report, shutting down.");
				}
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
			public RepoAndBranchInfo RepoAndBranchInfo { get; private set; }
			
			public BranchInfoReportedToAggregator(RepoAndBranchInfo branchName)
			{
				RepoAndBranchInfo = branchName;
			}
		}
	}

	public class Clear
	{
	}
}