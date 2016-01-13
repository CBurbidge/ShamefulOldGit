using System;

namespace ShamefulOldGit
{
	public class BranchInfo
	{
		public string Name { get; set; }
		public string CommitterEmail { get; set; }
		public string CommitterName { get; set; }
		public DateTimeOffset CommitterDate { get; set; }
		public string Message { get; set; }
		public string Sha { get; set; }
	}
}