# Azure Storage Example

This example shows how to use Azure Blob and File Storage.

To get this example working with your own Azure account, you will need to a few things:

# Step 1: Azure Portal
---
1. Create new file and blog storage containers

# Step 2: Update these items in the web.config file within the appSettings section
---
1. StorageConnectionString 
1. ShareName
1. BlobContainerName

Optionally, you can right click the project and left click "Manage User Secrets" and put your values in your secrets file.  The format looks like this:
```
<?xml version="1.0" encoding="utf-8"?>
<root>
   <secrets ver="1.0" >
      <secret name="StorageConnectionString" value="YourConnectionStringHereGetItFromThePortal" />
         <secret name="ShareName" value="YourFileShareNameHere" />
         <secret name="BlobContainerName" value="YourBlobContainerNameHere" />
   </secrets>
</root>
```

# Step 3: Using the application you can 
---
1. Upload files into both the file and blog storage.

