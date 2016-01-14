using System;
using System.IO;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class EmailingActor : ReceiveActor
	{
		private readonly IActorRef _lastEmailedFileActor;

		public EmailingActor(IActorRef lastEmailedFileActor)
		{
			_lastEmailedFileActor = lastEmailedFileActor;
			Receive<PrinterActor.EmailContentToBeSent>(message =>
			{
				var path = Path.GetFullPath("..\\..\\..\\Email.html");
				File.WriteAllText(path, message.Content);
				Console.WriteLine($"Email file saved to location {path}");

				_lastEmailedFileActor.Tell(new EmailedAtTime(DateTime.Now));
			});
		}

		public class EmailedAtTime
		{
			public DateTime Now { get; set; }

			public EmailedAtTime(DateTime now)
			{
				Now = now;
			}
		}
	}

	
}