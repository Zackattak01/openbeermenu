namespace OpenBeerMenu.Types.Sync
{
    public class SyncStatus
    {
        public static SyncStatus Success => new SyncStatus(false, "Up to date", string.Empty);
        public static SyncStatus Waiting => new SyncStatus(false, "Waiting for changes", string.Empty);
        public static SyncStatus Scheduled => new SyncStatus(false, "Sync Scheduled", string.Empty);
        public static SyncStatus Failed(string description) => new SyncStatus(true, "Failed", description);

        public bool IsFailure { get; }
        public string Message { get; } 
        public string Description { get; }
        public DateTime Timestamp { get; }

        private SyncStatus(bool isFailure, string message, string description)
        {
            IsFailure = isFailure;
            Message = message;
            Description = description;
            Timestamp = DateTime.Now;
        }

        
    }
}
