using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class RepositoriesCoordinatorActor : ReceiveActor
	{
		public List<string> _allRepositories; 

		public RepositoriesCoordinatorActor()
		{
			Receive<RepositoriesToReportOn>(message =>
			{
				_allRepositories = message.RepositoryPaths.ToList();

				Console.WriteLine("Received repos to analyse.");
				foreach (var repositoryPath in message.RepositoryPaths)
				{
					var repositoryActorName = repositoryPath.Split('\\').Last();
					var repoActor = Context.ActorOf(Props.Create(() => new RepositoryActor(repositoryPath, null)), repositoryActorName);
					repoActor.Tell(new Go());
				}
			});

			Receive<RepositoryActor.NoOldNoMergedBranchesInRepository>(message =>
			{
				Console.WriteLine($"No appropriate branches in repo {message.DirPath}");
				_allRepositories.Remove(message.DirPath);
			});

			Receive<RepositoryActor.RepositoryAllAccountedFor>(message =>
			{
				_allRepositories.Remove(message.DirPath);

				if (_allRepositories.Count == 0)
				{
					Console.WriteLine("All Repos are accounted for.");
					Context.ActorSelection(ActorSelectionRouting.BranchInfoAggregationActorPath)
						.Tell(new AllRepositoriesAccountedFor());
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
}