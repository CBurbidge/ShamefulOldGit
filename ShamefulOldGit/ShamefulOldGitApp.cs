using Akka.Actor;
using ShamefulOldGit.Actors;
using Topshelf;

namespace ShamefulOldGit
{
	public class ShamefulOldGitApp : ServiceControl
	{
		private readonly string[] _repositoryPaths =
		{
			@"C:\Dev\Abide\Abide.Digby"
		};

		private readonly ActorSystem MyActorSystem;

		public ShamefulOldGitApp()
		{
			MyActorSystem = ActorSystem.Create(ActorSelectionRouting.ActorSystemName);
		}

		public bool Start(HostControl hostControl)
		{
			var shutdownActor = MyActorSystem.ActorOf(
				Props.Create<ShutDownActor>(), 
				ActorSelectionRouting.ShutdownActorName);

			var printerActor = MyActorSystem.ActorOf(
				Props.Create<PrinterActor>(), 
				ActorSelectionRouting.PrinterActorName);

			var branchInfoAggregationActor = MyActorSystem.ActorOf(
				Props.Create<BranchInfoAggregationActor>(),
				ActorSelectionRouting.BranchInfoAggregationActorName);

			var repoCoord = MyActorSystem.ActorOf(
				Props.Create(
					() => new RepositoriesCoordinatorActor()),
				ActorSelectionRouting.RepositoriesCoordinatorActorName);

			var lastEmailedFileActor = MyActorSystem.ActorOf(
				Props.Create(
					() => new LastEmailedFileActor(_repositoryPaths)),
					"LastEmailedFileActor");

			var emailingActor = MyActorSystem.ActorOf(
				Props.Create(
					() => new EmailingActor(lastEmailedFileActor)),
				ActorSelectionRouting.EmailingActorName);

			lastEmailedFileActor.Tell(new Go());
			
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			MyActorSystem.Shutdown();
			return true;
		}
	}
}