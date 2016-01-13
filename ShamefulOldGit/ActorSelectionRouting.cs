using ShamefulOldGit.Actors;

namespace ShamefulOldGit
{
	public class ActorSelectionRouting
	{
		public const string ActorSystemName = "ShamefulOldGitActorSystem";
		private const string Template = "akka://" + ActorSystemName + "/user/";

		public const string RepositoriesCoordinatorActorName = "RepositoriesCoordinatorActor";
		public const string RepositoriesCoordinatorActorPath = Template + RepositoriesCoordinatorActorName;

		public const string BranchInfoAggregationActorName = "BranchInfoAggregationActor";
		public const string BranchInfoAggregationActorPath = Template + BranchInfoAggregationActorName;

		public const string PrinterActorName = "PrinterActor";
		public const string PrinterActorPath = Template + PrinterActorName;

		public const string EmailingActorName = "EmailingActor";
		public const string EmailingActorPath = Template + EmailingActorName;
	}
}