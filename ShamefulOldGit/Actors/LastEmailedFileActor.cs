using System;
using System.IO;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
	public class LastEmailedFileActor : ReceiveActor
	{
		private readonly string[] _repositoryPaths;
		private readonly string FileName = "LastEmailed.txt";
		const int HowManyDaysToWaitBeforeEmailAgain = 7;

		public LastEmailedFileActor(string[] repositoryPaths)
		{
			_repositoryPaths = repositoryPaths;
			Receive<Go>(message =>
			{
#if DEBUG
				if(File.Exists(FileName))File.Delete(FileName);
#endif
				if (File.Exists(FileName) == false)
				{
					TellToStart();
					return;
				}

				var lastEmailed = File.ReadAllText(FileName);
				DateTime lastEmailedDate;
				if (DateTime.TryParse(lastEmailed, out lastEmailedDate))
				{
					if (lastEmailedDate < DateTime.Now.AddDays(-1 * HowManyDaysToWaitBeforeEmailAgain))
					{
						TellToStart();
					}
					else
					{
						Console.WriteLine($"Was last run less than 7 days ago at {lastEmailed}");
					}
				}
				else
				{
					TellToStart();
				}
			});

			Receive<EmailingActor.EmailedAtTime>(message =>
			{
				File.WriteAllText(FileName, message.Now.ToString("O"));
				Console.WriteLine($"Email last write time file saved to {Path.GetFullPath(FileName)}");
			});
		}

		private void TellToStart()
		{
			Context.ActorSelection(ActorSelectionRouting.RepositoriesCoordinatorActorPath)
				.Tell(new RepositoriesCoordinatorActor.RepositoriesToReportOn(_repositoryPaths));
		}
	}
}