using Storage.Repositories.Utility;

namespace Storage.Repositories.Abstractions;

public interface IFileNameParser
{
    ParsedFileName Parse(string fileName);
}