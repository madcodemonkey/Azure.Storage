namespace Storage.Repositories
{
    public class FileStorageSettings
    {
        public FileStorageSettings()
        {
        }

        public FileStorageSettings(string connectionString, string shareName)
        {
            ConnectionString = connectionString;
            ShareName = shareName;
        }

        public string ConnectionString { get; set; }
        public string ShareName { get; set; }
    }
}