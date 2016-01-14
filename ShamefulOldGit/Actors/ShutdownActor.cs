using System;
using System.IO;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class ShutDownActor : ReceiveActor
	{
		// Can't get this to work properly...
		public ShutDownActor()
		{
			Receive<ShutItDown>(message =>
			{
				//Context.ActorSelection(ActorSelectionRouting.BranchInfoAggregationActorPath).Tell(PoisonPill.Instance);
				//Context.ActorSelection(ActorSelectionRouting.EmailingActorPath).Tell(PoisonPill.Instance);
				//Context.ActorSelection(ActorSelectionRouting.PrinterActorPath).Tell(PoisonPill.Instance);
				//Context.ActorSelection(ActorSelectionRouting.RepositoriesCoordinatorActorPath).Tell(PoisonPill.Instance);
				//Self.Tell(PoisonPill.Instance);
			});
		}

		protected override void PostStop()
		{
			base.PostStop();

			Context.System.Shutdown();
		}
	}

	public class ShutItDown
	{
	}
}