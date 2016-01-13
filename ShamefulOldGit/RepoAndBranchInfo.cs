namespace ShamefulOldGit
{
	public class RepoAndBranchInfo
	{
		public string DirPath;
		public BranchInfo BranchInfo;

		public RepoAndBranchInfo(string dirPath, BranchInfo branchInfo)
		{
			DirPath = dirPath;
			BranchInfo = branchInfo;
		}
	}
}