using System;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class EmailingActor : ReceiveActor
	{
		public EmailingActor()
		{
			Receive<PrinterActor.EmailContentToBeSent>(message =>
			{
				Console.WriteLine("Email going to be sent.");
			});
		}
	}
}