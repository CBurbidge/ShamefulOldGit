using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace ShamefulOldGit.Actors
{
    public class RepositoriesCoordinatorActor : ReceiveActor
	{
		public class RepositoriesToReportOn
		{
			public RepositoriesToReportOn(IReadOnlyCollection<string> repositoryPaths)
			{
				RepositoryPaths = repositoryPaths;
			}

			public IReadOnlyCollection<string> RepositoryPaths { get; private set; }  
		}
	    public RepositoriesCoordinatorActor()
	    {
			Console.WriteLine("Initiated Repo coordinator.");

		    Receive<RepositoriesToReportOn>(message =>
		    {
				Console.WriteLine("Received repos to analyse.");
			    foreach (var repositoryPath in message.RepositoryPaths)
			    {
				    var dirName = repositoryPath.Split('\\').Last();
				    var repoActor = Context.ActorOf(Props.Create(() => new RepositoryActor(repositoryPath, null)), dirName);
					repoActor.Tell(new Go());
			    }
		    });
	    }
	}
}
