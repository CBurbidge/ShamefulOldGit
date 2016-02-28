using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class PrinterActor : ReceiveActor
	{
		public PrinterActor()
		{
			Receive<BranchInfoAggregationActor.BranchInfosToPrint>(message =>
			{
				Logger.WriteLine("Printing out brancheInfos.");
				var emailBuilder = new EmailBuilder();

				var emailContent = emailBuilder.BuildSummaryEmail(message.ReposAndBranchInfos);

				Context.ActorSelection(ActorSelectionRouting.EmailingActorPath)
					.Tell(new EmailContentToBeSent(emailContent));
			});
		}

		public class EmailContentToBeSent
		{
			public EmailContentToBeSent(string content)
			{
				Content = content;
			}

			public string Content { get; }
		}
	}
}