namespace Storage.Repositories
{
    public class BlobStorageSettings
    {
        public BlobStorageSettings()
        {
        }

        public BlobStorageSettings(string connectionString, string containerName)
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
        }

        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}