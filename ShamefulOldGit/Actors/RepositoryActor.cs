using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using LibGit2Sharp;

namespace ShamefulOldGit.Actors
{
	public class RepositoryActor : ReceiveActor
	{
		private readonly IActorRef _gitBranchActor;
		private readonly Func<DateTime> _getDateTimeNow;
		private List<string> _oldNoMergedBranchNames;
		private readonly string[] _exceptions =
		{
			"z-archive/"
		};

		public RepositoryActor(IActorRef gitBranchActor, Func<DateTime> getDateTimeNow)
		{
			_gitBranchActor = gitBranchActor;
			_getDateTimeNow = getDateTimeNow ?? (() => DateTime.Now);

			Receive<StartRepository>(message =>
			{
				Logger.WriteLine($"Starting repo actor {message.RepositoryPath}");
				using (var repository = new Repository(message.RepositoryPath))
				{
					var comparisonBranch = repository.Branches[Constants.ComparisonBranchName];
					var comparisonBranchCommitShas = comparisonBranch.Commits.Select(c => c.Sha).ToList();

					var oldNoMergedBranches = repository.Branches.Where(b => b.Commits.Any()
																			  && b.IsRemote
																			  && BranchNameDoesntContainExceptionString(b)
																			  && BranchIsOld(b)
																			  && BranchIsNoMerged(comparisonBranchCommitShas, b))
						.ToList();

					if (oldNoMergedBranches.Count == 0)
					{
						Sender.Tell(new NoOldNoMergedBranchesInRepository(message.RepositoryPath));
					}

					_oldNoMergedBranchNames = oldNoMergedBranches.Select(b => b.Name).ToList();

					foreach (var branch in oldNoMergedBranches)
					{
						Sender.Tell(new RegisterBranch(message.RepositoryPath, branch.Name));
						_gitBranchActor.Tell(new BranchInfoActor.GetBranchInfo(message.RepositoryPath, message.RepositoryPath, branch.Name));
					}
				}
			});

			
		}

		private bool BranchNameDoesntContainExceptionString(Branch b)
		{
			var anyOfTheExceptionsAreInTheBranchName = _exceptions.Any(e => b.Name.Contains(e));
			return anyOfTheExceptionsAreInTheBranchName == false;
		}

		private bool BranchIsOld(Branch branch)
		{
			var timeLimit = _getDateTimeNow().AddMonths(-1 *Constants.MonthsPriorToNow);
			var branchIsOld = branch.Tip.Committer.When < timeLimit;
			return branchIsOld;
		}

		private static bool BranchIsNoMerged(List<string> comparisonBranch, Branch branchToCompare)
		{
			return comparisonBranch.Contains(branchToCompare.Tip.Sha) == false;
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

	public class RegisterBranch
	{
		public string RepositoryPath { get; set; }
		public string BranchName { get; set; }

		public RegisterBranch(string repositoryPath, string branchName)
		{
			RepositoryPath = repositoryPath;
			BranchName = branchName;
		}
	}
}