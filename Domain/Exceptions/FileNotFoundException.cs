namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a file is not found in storage
/// </summary>
public class StorageFileNotFoundException : DomainException
{
    public StorageFileNotFoundException(string objectKey) 
        : base($"File with object key '{objectKey}' not found in storage. Please upload the file again.")
    {
    }
}

