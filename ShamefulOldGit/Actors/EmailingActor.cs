using System;
using System.IO;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class EmailingActor : ReceiveActor
	{
		public EmailingActor()
		{
			Receive<PrinterActor.EmailContentToBeSent>(message =>
			{
				var path = Path.GetFullPath("..\\..\\..\\Email.html");
				File.WriteAllText(path, message.Content);
				Console.WriteLine($"File saved to location {path}");
				Console.WriteLine("Press any key to stop actor system.");
				Console.ReadKey();

				Context.System.Shutdown();
				Console.ReadKey();
			});
		}
	}
}