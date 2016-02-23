using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class RepositoriesCoordinatorActor : ReceiveActor
	{
		private List<string> _allRepositories;
		private Dictionary<string, List<string>> _branchesByRepositoryPath = new Dictionary<string, List<string>>();
		private readonly IActorRef _repositoryActor;
		public RepositoriesCoordinatorActor(IActorRef repositoryActor)
		{
			_repositoryActor = repositoryActor;
			Receive<RepositoriesToReportOn>(message =>
			{
				_allRepositories = message.RepositoryPaths.ToList();

				Logger.WriteLine("Received repos to analyse.");
				foreach (var repositoryPath in message.RepositoryPaths)
				{
					_repositoryActor.Tell(new StartRepository(repositoryPath));
				}
			});

			Receive<RepositoryActor.NoOldNoMergedBranchesInRepository>(message =>
			{
				Logger.WriteLine($"No appropriate branches in repo {message.DirPath}");
				_allRepositories.Remove(message.DirPath);
			});

			Receive<RegisterBranch>(message =>
			{
				if (_branchesByRepositoryPath.ContainsKey(message.RepositoryPath) == false)
				{
					_branchesByRepositoryPath.Add(message.RepositoryPath, new List<string>());
				}

				_branchesByRepositoryPath[message.RepositoryPath].Add(message.BranchName);
			});

			Receive<RepositoryActor.RepositoryAllAccountedFor>(message =>
			{
				_allRepositories.Remove(message.DirPath);

				if (_allRepositories.Count == 0)
				{
					Logger.WriteLine("All Repos are accounted for.");
					Context.ActorSelection(ActorSelectionRouting.BranchInfoAggregationActorPath)
						.Tell(new AllRepositoriesAccountedFor());
				}
			});

			Receive<BranchInfoAggregationActor.BranchInfoReportedToAggregator>(message =>
			{
				var repoBranches = _branchesByRepositoryPath[message.RepoAndBranchInfo.DirPath];
				repoBranches.Remove(message.RepoAndBranchInfo.BranchInfo.Name);
				if (repoBranches.Count == 0)
				{
					_branchesByRepositoryPath.Remove(message.RepoAndBranchInfo.DirPath);
					Self.Tell(new RepositoryActor.RepositoryAllAccountedFor(message.RepoAndBranchInfo.DirPath));
				}
			});
		}

		public class AllRepositoriesAccountedFor
		{
		}

		public class RepositoriesToReportOn
		{
			public RepositoriesToReportOn(IReadOnlyCollection<string> repositoryPaths)
			{
				RepositoryPaths = repositoryPaths;
			}

			public IReadOnlyCollection<string> RepositoryPaths { get; }
		}
	}

	public class StartRepository
	{
		public string RepositoryPath { get; set; }

		public StartRepository(string repositoryPath)
		{
			RepositoryPath = repositoryPath;
		}
	}
}