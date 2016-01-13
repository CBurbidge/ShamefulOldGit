using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using LibGit2Sharp;

namespace ShamefulOldGit.Actors
{
	public class RepositoryActor : ReceiveActor
	{
		private readonly Func<DateTime> _getDateTimeNow;
		private const int MonthsPriorToNow = 0;
		private const string ComparisonBranchName = "origin/develop";
		private readonly Repository _repository;
		private List<string> _oldNoMergedBranchNames;

		public RepositoryActor(string dirPath, Func<DateTime> getDateTimeNow)
		{
			_getDateTimeNow = getDateTimeNow ?? (() => DateTime.Now);
			_repository = new Repository(dirPath);

			Receive<Go>(message =>
			{
				Console.WriteLine($"Starting repo actor {dirPath}");

				var comparisonBranch = _repository.Branches[ComparisonBranchName];

				var oldNoMergedBranches = _repository.Branches.Where(b => b.Commits.Any()
				                                                  && b.IsRemote
				                                                  && BranchIsOld(b)
				                                                  && BranchIsNoMerged(comparisonBranch, b)).ToList();

				if (oldNoMergedBranches.Count == 0)
				{
					Sender.Tell(new NoOldNoMergedBranchesInRepository());
				}

				_oldNoMergedBranchNames = oldNoMergedBranches.Select(b => b.Name).ToList();

				foreach (var branch in oldNoMergedBranches)
				{
					var branchNameSuitableForAnActor = branch.Name
						.Replace(@"/", "");
					var branchActor = Context.ActorOf(Props.Create<BranchInfoActor>(), branchNameSuitableForAnActor);
					branchActor.Tell(new BranchInfoActor.GetBranchInfo(_repository, branch));
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

		public class NoOldNoMergedBranchesInRepository
		{
		}
	}
}