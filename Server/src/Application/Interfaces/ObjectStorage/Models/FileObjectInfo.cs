namespace Application.Interfaces.ObjectStorage.Models
{
    public sealed record FileObjectInfo(bool Exists, long? SizeInBytes = null);
}
