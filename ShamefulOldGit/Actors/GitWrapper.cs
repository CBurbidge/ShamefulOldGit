using System;
using System.Diagnostics;

namespace ShamefulOldGit.Actors
{
	public class GitWrapper
	{
		// The libgit doesn't seem to offer this so spin up a process.
		public void FetchThenSafePrune(string repoPath)
		{
			Fetch(repoPath);

			Logger.WriteLine($"Going to prune {repoPath}");

			var arguments = "remote prune origin";
			RunCommand(repoPath, arguments);
		}

		private static void RunCommand(string repoPath, string arguments)
		{

			try
			{
				using (var process = new Process
				{
					StartInfo =
					{
						FileName = "git",
						Arguments = arguments,
						WorkingDirectory = repoPath,
						UseShellExecute = false,
						RedirectStandardError = true,
						RedirectStandardOutput = true
					}
				})
				{

					var success = process.Start();
					var result = process.StandardOutput.ReadToEnd();
					var error = process.StandardError.ReadToEnd();
					Logger.WriteLine(success ? "Success" : "Failure");
					if (string.IsNullOrEmpty(result) == false)
					{
						Logger.WriteLine($"StandardOutput - " + result);
					}
					if (string.IsNullOrEmpty(error) == false)
					{
						Logger.WriteLine($"StandardError - " + error);
					}
				}
			}
			catch (Exception e)
			{
				Logger.WriteLine($"Failed - {e.Message}");
			}

			Logger.SaveToFile();
		}

		// Could do this later on but want to fetch then prune then check stuff so doing it here.
		private void Fetch(string repositoryPath)
		{
			Logger.WriteLine($"Going to fetch {repositoryPath}");

			var arguments = "fetch";
			RunCommand(repositoryPath, arguments);
		}
	}
}