# net.Core.MongoDB.FileServer

A .net core web server for storing and retrieving files.

### Get Started

You should have installed mongo database first.

1.  Clone the project
2.  Hit F5
3.  Inspect the controller. You will find two methods **upload-file** & **download-file**
4.  Upload your first file.  **Do a POST to api/Files/upload-file**. Notice that the parameter folderId is optional.
   * Headers: Content-Type=application/x-www-form-urlencoded
   * Body: select a key with any name but type=file and then choose your file
5. If everything goes well you wil get the url of the file you have just uploaded in the response
6. Copy the fileUrl and **Do a GET request** to download it.
7. Inspect the mongo database that has been created. *Database Name=Directory, Collection=Folders*. The file is being stored in the files collection (of Directory) and its reference can be found in the Files array of the Folders collection.
