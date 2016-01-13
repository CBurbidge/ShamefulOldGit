using System;
using Akka.Actor;
using LibGit2Sharp;

namespace ShamefulOldGit.Actors
{
	public class BranchInfoActor : ReceiveActor
	{
		public BranchInfoActor()
		{
			Receive<GetBranchInfo>(message =>
			{
				Console.WriteLine($"Starting Branch info {message.Repository.Info.Path} -> {message.Branch.Name}");
				//Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));

			});
		}

		public class GetBranchInfo
		{
			public GetBranchInfo(Repository repository, Branch branch)
			{
				Repository = repository;
				Branch = branch;
			}

			public Repository Repository { get; private set; }
			public Branch Branch { get; private set; }
		}
	}
}