using System;
using System.Diagnostics;

namespace ShamefulOldGit.Actors
{
	public class Pruner
	{
		// The libgit doesn't seem to offer this so spin up a process.
		public void SafePrune(string repoPath)
		{
			Logger.WriteLine($"Going to prune {repoPath}");

			var process = new Process
			{
				StartInfo =
				{
					FileName = "git",
					Arguments = "remote prune origin",
					WorkingDirectory = repoPath,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};

			try
			{
				var success = process.Start();
				var result = process.StandardOutput.ReadToEnd();
				var error = process.StandardError.ReadToEnd();
				Logger.WriteLine(success ? "Success" : "Failure");
				Logger.WriteLine(success ? result : error);
			}
			catch (Exception e)
			{
				Logger.WriteLine($"Failed - {e.Message}");
			}

			Logger.SaveToFile();
		}
	}
}