using System.Collections.Generic;

public interface IDataHandler
{
    IReadOnlyCollection<string> SupportedDataTypes { get; }
    void HandleData(string dataType, byte[] data);
}