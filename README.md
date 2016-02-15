#PackStream.NET

**Alpha** 

[![cskardon MyGet Build Status](https://www.myget.org/BuildSource/Badge/cskardon?identifier=8193f901-2cc2-4faf-8b71-c5bccdc01b32)](https://www.myget.org/)

## What???

PackStream.NET is a .NET implementation of the *PackStream* protocol devised by [Neo4j](http://alpha.neohq.net/docs/driver-manual/#term-bolt). 
This is intended to be used with a *Bolt* based driver for Neo4j, but there is nothing stopping you using it in any other client/server situation.

### OK How do I use?

* NuGet at: [PackStream.NET](https://nuget.org/packages/PackStream.NET)
* MyGet at: [PackStream.NET](https://www.myget.org/F/cskardon/api/v3/index.json)

Most of the examples are in the unit tests, but simply:

### Pack an object

```csharp
public class Movie {
    public string Title { get; set; }
    public string TagLine { get; set; }
    public int Released { get; set; }
}

var packMe = new Movie{Title = "Jurassic Park", Released = 1994, TagLine = "Something Dinosaury"};
byte[] bytes = PackStream.Pack(packMe);

var unpacked = PackStream.Unpack<Movie>(bytes);
unpacked.Title.Should().Be("Jurassic Park");
```
