# Blob Uploader Example

This example shows how to use upload a blob and add metadata as you upload the files.  

It will upload all the files in the specified directory that ends with 'UploadFiles' (see UploadFilesToBlobStorageMenuItem.cs to change that).  
It will search the BIN directory where the EXE file is located and keep going all the way up to the root of the drive looking for a directory 
that ends with 'UploadFiles'.  If it's not found it will tell you and stop.

To get this example working with your own Azure account, you will need to a few things:

## Step 1: Azure Portal
1. Create a storage account
1. Create a new blob storage container (under containers)
1. Under 'Access Key' in the portal, get a storage connection string.

## Step 2: Update these items in the appsettings.json file
1. StorageConnectionString 
1. BlobContainerName

Optionally, you can right click the project and left click "Manage User Secrets" and put your values in your secrets file.  The format looks like this:
```json
{
  "StorageConnectionString": "YourConnectionStringHereGetItFromThePortal",
  "BlobContainerName": "YourBlobContainerNameHere"
}
```

## Step 3: files needed
1. Put a bunch of files in a directory called 'UploadFiles' at the same location as the solution file.

## Step 4: Run it
1. Run the application
1. Use option one and follow the on-screen prompts.



