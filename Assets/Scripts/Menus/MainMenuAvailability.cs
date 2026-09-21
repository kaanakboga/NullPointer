namespace NullPointer.Menus
{
    public static class MainMenuAvailability
    {
        public static bool CanContinue(IGameSessionCommands commands)
        {
            return commands != null && commands.HasContinue;
        }
    }
}
