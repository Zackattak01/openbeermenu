namespace OpenBeerMenu.Types.DragAndDrop
{
    public class DragAndDropTargetInfo : IEquatable<DragAndDropTargetInfo>
    {
        public Guid Id { get; init; }
        public object Data { get; init; }

        public bool Equals(DragAndDropTargetInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DragAndDropTargetInfo)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}