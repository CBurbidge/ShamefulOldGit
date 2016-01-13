using System;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class PrinterActor : ReceiveActor
	{
		public PrinterActor()
		{
			Receive<BranchInfoAggregationActor.BranchInfosToPrint>(message =>
			{
				Console.WriteLine("Printing out brancheInfos.");
				var emailContent = "This is some email jazz.";

				Context.ActorSelection(ActorSelectionRouting.EmailingActorPath)
					.Tell(new EmailContentToBeSent(emailContent));
			});
		}

		public class EmailContentToBeSent
		{
			public string Content { get; }

			public EmailContentToBeSent(string content)
			{
				Content = content;
			}
		}
	}
}