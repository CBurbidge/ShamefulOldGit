﻿using Akka.Actor;
using ShamefulOldGit.Actors;
using Topshelf;

namespace ShamefulOldGit
{
	public class ShamefulOldGitApp : ServiceControl
	{
		private readonly string[] _repositoryPaths =
		{
			@"C:\Dev\Abide\Abide.Digby",
			@"C:\Dev\Abide\Abide.Gaspode",
		};

		private readonly ActorSystem MyActorSystem;

		public ShamefulOldGitApp()
		{
			MyActorSystem = ActorSystem.Create(ActorSelectionRouting.ActorSystemName);
		}

		public bool Start(HostControl hostControl)
		{
			var printerActor = MyActorSystem.ActorOf(
				Props.Create<PrinterActor>(), 
				ActorSelectionRouting.PrinterActorName);

			var branchInfoAggregationActor = MyActorSystem.ActorOf(
				Props.Create<BranchInfoAggregationActor>(),
				ActorSelectionRouting.BranchInfoAggregationActorName);

			var branchActor = MyActorSystem.ActorOf(
				Props.Create<BranchInfoActor>(), 
				ActorSelectionRouting.BranchActorName);

			var repositoryActor = MyActorSystem.ActorOf(
				Props.Create(() => new RepositoryActor(branchActor, null)),
				ActorSelectionRouting.RepositoryActorName);
			
			var repoCoord = MyActorSystem.ActorOf(
				Props.Create(
					() => new RepositoriesCoordinatorActor(repositoryActor)),
				ActorSelectionRouting.RepositoriesCoordinatorActorName);

			var lastEmailedFileActor = MyActorSystem.ActorOf(
				Props.Create(
					() => new LastEmailedFileActor(_repositoryPaths)),
					"LastEmailedFileActor");

			var emailingActor = MyActorSystem.ActorOf(
				Props.Create(
					() => new EmailingActor(lastEmailedFileActor)),
				ActorSelectionRouting.EmailingActorName);
			
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			MyActorSystem.Shutdown();
			return true;
		}
	}
}