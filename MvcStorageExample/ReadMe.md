# Azure Storage Example

This example shows how to use Azure Blob and File Storage.

To get this example working with your own Azure account, you will need to a few things:

## Step 1: Azure Portal
1. Create a storage account
1. Create new file share in teh storage account 
1. Create a new blob storage container (under containers)
1. Generate out a new Shared Access Signature (don't forget to allow both "Container" and "Object"); afterwards, it should generate a new SAS and connection string.

## Step 2: Update these items in the appsettings.json file
1. StorageConnectionString 
1. ShareName
1. BlobContainerName

Optionally, you can right click the project and left click "Manage User Secrets" and put your values in your secrets file.  The format looks like this:
```json
{
  "StorageConnectionString": "YourConnectionStringHereGetItFromThePortal",
  "ShareName": "YourFileShareNameHere",
  "BlobContainerName": "YourBlobContainerNameHere"
}
```

## Step 3: Using the application you can 
1. Upload files into both the file and blog storage.

