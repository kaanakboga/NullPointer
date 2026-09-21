namespace NullPointer.Terminal
{
    public readonly struct TerminalEntryOpened
    {
        public TerminalEntryOpened(TerminalData terminal, TerminalEntry entry)
        {
            Terminal = terminal;
            Entry = entry;
        }

        public TerminalData Terminal { get; }

        public TerminalEntry Entry { get; }
    }
}
