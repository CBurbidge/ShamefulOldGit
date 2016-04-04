using System.IO;

namespace ShamefulOldGit
{
	public class Paths
	{
		public static string GetTopPath(string path)
		{
			return Path.GetFullPath($"..\\..\\..\\{path}");
		}
	}
}