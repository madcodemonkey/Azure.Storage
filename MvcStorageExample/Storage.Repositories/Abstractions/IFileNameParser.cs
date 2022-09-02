namespace Storage.Repositories;

public interface IFileNameParser
{
    ParsedFileName Parse(string fileName);
}