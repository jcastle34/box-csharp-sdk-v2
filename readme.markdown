# Box C# SDK (API v2)

This is a C# client implementation of the [Box v2 REST API](http://developers.box.com/docs/).  Please feel free to open an issue if you find a bug or would like to request a feature.

## License

[Creative Commons Attribution 3.0 Unported License](http://creativecommons.org/licenses/by/3.0/)

## Nuget

This client is [available on Nuget](http://nuget.org/packages/Box.v2.SDK).  There is also an [MVC-based example](https://github.com/jhoerr/box-csharp-sdk-v2.sample.oauth) of the Box OAuth2 authentication flow available.

## Release Notes

###1.3
 + Feature: Search
 + Feature: File thumbnails
 + Feature: File/Folder path collections now provided
 + Improvement: Strings are now trimmed, which prevents request problems in certain circumstances
 + Bug: File/Folder size would fail to deserialize when very large
 + Bug: Boolean values in query strings were improperly serialized
 + Breaking Change: File/Folder Path and PathId have been removed from the API
 + Breaking Change: File/Folder Size is now a double (was an int)
 
###1.2
 + Improvement: Now honoring the HTTP 202 <em>Retry-After</em> header when attempting to download file contents.
 + Improvement: Faster performance when uploading files.

###1.1
 + Breaking Change: The BoxManager constructor signatures have changed to resolve ambigious method call errors in Visual Studio.
 + Breaking Change: The enterprise-level User methods have all changed to resolve an issue that could potentially lead to data loss.
 + Bug: BoxException information is now properly saved.

###1.0
 + Initial release 

## Known Issues

+ Support for several features is planned but not yet available, including removing a user from an enterprise.
+ Long-polling of events is not currently supported due to limitations in the underlying request model.

## Usage Example

```csharp
// Instantiate a BoxManager client.
var boxManager = new BoxManager("AccessToken");

// Optionally refresh the access token (they are only good for an hour!)
// You may want to persist these new values for later use.
var refreshedAccessToken = boxManager.RefreshAccessToken();

// Create a new file in the root folder
boxManager.CreateFile(Folder.Root, "a new file.txt", Encoding.UTF8.GetBytes("hello, world!"));

// Fetch the root folder
var folder = boxManager.GetFolder(Folder.Root);

// Find a 'mini' representation of the created file among the root folder's contents
var file = folder.Files.Single(f => f.Name.Equals("a new file.txt"));

// Get the file with all properties populated.
file = boxManager.Get(file);

// Rename the file
file = boxManager.Rename(file, "the new name.txt");

// Create a new subfolder
var subfolder = boxManager.CreateFolder(Folder.Root, "my subfolder");

// Move the file to the subfolder
file = boxManager.Move(file, subfolder);

// Write some content to the file
using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("goodbye, world!")))
{
    file = boxManager.Write(file, stream);
}

// Read the contents to a stream and write them to the console
using (var stream = new MemoryStream())
{
    boxManager.Read(file, stream);
    using (var reader = new StreamReader(stream))
    {
        stream.Position = 0;
        Console.Out.WriteLine("File content: '{0}'", reader.ReadToEnd());
    }
}

// Delete the folder and its contents
boxManager.Delete(subfolder, recursive: true);

## Copyright

(c) 2012-2013 John Hoerr, The Trustees of Indiana University
