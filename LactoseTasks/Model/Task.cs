namespace LactoseTasks.Model;

public class Goal
{
    public required string GoalName { get; set; }
    public required string GoalEventId { get; set; }
    public required double RequiredGoalValue { get; set; }
}

public class Task
{
    public required string TaskId { get; set; }
    public required string TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public IList<string>? Rewards { get; set; }
    public required IList<Goal> Goals { get; set; }
}