namespace LactoseTasks.Model;

public class PlayerGoalProgress
{
    public required double GoalValue { get; set; }
}

public class PlayerTaskProgress
{
    public required string PlayerTaskProgressId { get; set; }
    public required string TaskId { get; set; }
    public required IList<PlayerGoalProgress> GoalsProgress { get; set; }
    public required bool Completed { get; set; }
}