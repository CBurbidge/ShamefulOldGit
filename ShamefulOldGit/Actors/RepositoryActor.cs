using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using LibGit2Sharp;

namespace ShamefulOldGit.Actors
{
	public class RepositoryActor : ReceiveActor
	{
		public const int MonthsPriorToNow = 0;
		public const string ComparisonBranchName = "origin/develop";
		private readonly string _dirPath;
		private readonly Func<DateTime> _getDateTimeNow;
		private readonly Repository _repository;
		private List<string> _oldNoMergedBranchNames;

		public RepositoryActor(string dirPath, Func<DateTime> getDateTimeNow)
		{
			_dirPath = dirPath;
			_getDateTimeNow = getDateTimeNow ?? (() => DateTime.Now);
			_repository = new Repository(dirPath);

			Receive<Go>(message =>
			{
				Console.WriteLine($"Starting repo actor {dirPath}");

				var comparisonBranch = _repository.Branches[ComparisonBranchName];

				var oldNoMergedBranches = _repository.Branches.Where(b => b.Commits.Any()
				                                                          && b.IsRemote
				                                                          && BranchIsOld(b)
				                                                          && BranchIsNoMerged(comparisonBranch, b))
					.ToList();

				if (oldNoMergedBranches.Count == 0)
				{
					Sender.Tell(new NoOldNoMergedBranchesInRepository(dirPath));
				}

				_oldNoMergedBranchNames = oldNoMergedBranches.Select(b => b.Name).ToList();

				foreach (var branch in oldNoMergedBranches)
				{
					var branchNameSuitableForAnActor = branch.Name
						.Replace(@"/", "");
					var branchActor = Context.ActorOf(Props.Create<BranchInfoActor>(), branchNameSuitableForAnActor);
					branchActor.Tell(new BranchInfoActor.GetBranchInfo(_dirPath, _repository, branch));
				}
			});

			Receive<BranchInfoAggregationActor.BranchInfoReportedToAggregator>(message =>
			{
				_oldNoMergedBranchNames.Remove(message.BranchName);

				if (_oldNoMergedBranchNames.Count == 0)
				{
					Console.WriteLine($"There are no more branches to report for repo {_dirPath}");
					Context.ActorSelection(ActorSelectionRouting.RepositoriesCoordinatorActorPath)
						.Tell(new RepositoryAllAccountedFor(_dirPath));
				}
			});
		}

		private bool BranchIsOld(Branch branch)
		{
			return branch.Commits.Last().Committer.When < _getDateTimeNow().AddMonths(-1*MonthsPriorToNow);
		}

		private static bool BranchIsNoMerged(Branch comparisonBranch, Branch branchToCompare)
		{
			return comparisonBranch.Commits.Contains(branchToCompare.Tip) == false;
		}

		protected override void PostStop()
		{
			// Repository is an IDisposable class for some reason.
			_repository.Dispose();

			base.PostStop();
		}

		public class RepositoryAllAccountedFor
		{
			public RepositoryAllAccountedFor(string dirPath)
			{
				DirPath = dirPath;
			}

			public string DirPath { get; set; }
		}

		public class NoOldNoMergedBranchesInRepository
		{
			public string DirPath { get; set; }

			public NoOldNoMergedBranchesInRepository(string dirPath)
			{
				DirPath = dirPath;
			}
		}
	}
}