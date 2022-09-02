# Azure Storage Example

This example shows how to use Azure Blob and File Storage.

To get this example working with your own Azure account, you will need to a few things:

## Step 1: Azure Portal
1. Create new file and blob storage containers

## Step 2: Update these items in the appSettings.json file
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

