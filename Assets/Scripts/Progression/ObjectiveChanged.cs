namespace NullPointer.Progression
{
    public readonly struct ObjectiveChanged
    {
        public ObjectiveChanged(ObjectiveData objective, bool isCompleted)
        {
            Objective = objective;
            IsCompleted = isCompleted;
        }

        public ObjectiveData Objective { get; }

        public bool IsCompleted { get; }
    }
}
