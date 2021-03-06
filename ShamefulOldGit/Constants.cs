namespace ShamefulOldGit
{
	public class Constants
	{
		public const bool SendEmails = true;

		public const int MonthsPriorToNow = 3;
		public const string ComparisonBranchName = "origin/develop";

		public const int SetTimeoutNumberOfSeconds = 60 * 60;
		public const int HowManyDaysToWaitBeforeEmailAgain = 7;

		public const int EmailCommitBranchNameLength = 50;
		public const int EmailCommitMessageLength = 50;
		public const int EmailNumberOfCharsOfShaToDisplay = 6;
	}
}