using System;
using System.Collections.Generic;
using System.IO;

namespace ShamefulOldGit.Actors
{ 
	/// <summary>
	/// Yeah, I went there.
	/// </summary>
	public class Logger
	{
		private static List<string> _log = new List<string>(); 

		public static void WriteLine(string line)
		{
			Console.WriteLine(line);
			_log.Add($"{DateTime.Now.ToString("O")} - {line}");
		}
		public static void SaveToFile()
		{
			var filePath = Path.GetFullPath($"..\\..\\..\\Log.log");
			var str = string.Join(Environment.NewLine, _log);

			if (File.Exists(filePath))
			{
				var info = new FileInfo(filePath);

				const long MaxLength = 1024*1024*10; // 10mb is probs fine?

				if (info.Length > MaxLength)
				{
					File.WriteAllText(filePath, str);
				}
				else
				{
					var content = File.ReadAllText(filePath);
					var oldPlusNew = content + Environment.NewLine + str;
					File.WriteAllText(filePath, oldPlusNew);
				}
			}
			else
			{
				File.WriteAllText(filePath, str);
			}

			_log.Clear();
		}
	}
}