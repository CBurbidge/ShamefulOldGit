using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace ShamefulOldGit.App
{
	class Program
	{
		static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(s => new ShamefulOldGitApp());

				x.RunAsLocalSystem();

				x.RunAsLocalService();

				x.StartAutomatically();

				x.SetServiceName("ShamefulOldGit");
				x.SetDisplayName("ShamefulOldGit");
				x.SetDescription("Shaming old git commits.");
			});
			
		}
	}
}
