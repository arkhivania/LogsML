namespace LogBins.Tests.Tools
{
    class ER
    {
        public ER(ulong entryAddress, string message)
        {
            EntryAddress = entryAddress;
            Message = message;
        }

        public ulong EntryAddress { get; }
        public string Message { get; }
    }
}
