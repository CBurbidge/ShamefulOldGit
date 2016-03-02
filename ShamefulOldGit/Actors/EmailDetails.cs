using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Optional;
using Optional.Unsafe;

namespace ShamefulOldGit.Actors
{
	public class EmailDetails
	{
		public string To { get; set; }
		public string From { get; set; }
		public string Host { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public Dictionary<string, string> Exceptions { get; set; }

		public static Option<EmailDetails> GetFromFilePath(string filePath)
		{
			var emailDetailsFile = File.ReadAllText(filePath);
			var lines = emailDetailsFile.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			try
			{				
				var toLine = CheckStartOfString(1, lines[0], "to:");
				var fromLine = CheckStartOfString(2, lines[1], "from:");
				var hostLine = CheckStartOfString(3, lines[2], "host:");
				var usernameLine = CheckStartOfString(4, lines[3], "username:");
				var passwordLine = CheckStartOfString(5, lines[4], "password:");
				var exceptionsLine = CheckStartOfString(6, lines[5], "exceptions:");

				var exceptionsAndReplacements = exceptionsLine.Split(';').ToArray();
				if (exceptionsAndReplacements.Length % 2 != 0)
				{
					throw new InvalidProgramException("Pairs don't exist for exceptions");
				}

				var oldEmailAddresses = exceptionsAndReplacements.Where(e => Array.FindIndex(exceptionsAndReplacements, a => a == e) % 2 == 0).ToList();
				var newEmailAddresses = exceptionsAndReplacements.Where(e => Array.FindIndex(exceptionsAndReplacements, a => a == e) % 2 == 1).ToList();
				var exceptions = Enumerable.Range(0, oldEmailAddresses.Count).ToDictionary(i => oldEmailAddresses[i], i => newEmailAddresses[i]);

				var details = new EmailDetails
				{
					To = toLine,
					From = fromLine,
					Username = usernameLine,
					Host = hostLine,
					Password = passwordLine,
					Exceptions = exceptions 
				};

				return Option.Some(details);
			}
			catch (EmailDetailsFileIncorrect)
			{}

			return Option.None<EmailDetails>();
		}

		private static string CheckStartOfString(int lineNumber, string line, string start)
		{
			if (line.StartsWith(start) == false)
			{
				throw new EmailDetailsFileIncorrect(start, lineNumber);
			}
			return line.Replace(start, "");
		}

		public static EmailDetails Get()
		{
			var path = GetPath("EmailDetails.txt");
			return GetFromFilePath(path).ValueOrFailure("File is bad.");
		}

		private static string GetPath(string path)
		{
			return Path.GetFullPath($"..\\..\\..\\{path}");
		}
	}
}