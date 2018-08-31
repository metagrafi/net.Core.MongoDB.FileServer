# net.Core.MongoDB.FileServer

A .net core web server for storing and retrieving files.

### Get Started

You should have installed mongo database first.

1.  Clone the project
2.  Hit F5
3.  Inspect the controller. You wil find two methods **upload-file** & **download-file**
4.  Upload your first file.  **Do a POST to api/Files/upload-file**
   * Headers: Content-Type=application/x-www-form-urlencoded
   * Body: select a key with any name but type=file and then choose your file
5. If everything goes well you wil get the url of the file you have just uploaded in the response
6. Copy the fileUrl and **Do a GET request** to download it.
