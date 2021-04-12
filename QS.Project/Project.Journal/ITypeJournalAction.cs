namespace QS.Project.Journal
{
    public interface ITypeJournalAction : IJournalAction
    {
        JournalActionType ActionType { get; }
    }

    public enum JournalActionType
    {
        Select,
        Add,
        Edit,
        Delete
    }
}
