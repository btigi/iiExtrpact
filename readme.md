iiExtrpact
=========

iiExtrpact is a C# library for unpacking RPA archives as used in numerous visual novels.

## Usage

```
  var rpaFile = @"D:\game\archive.rpa";
  var outputDir = @"D:\game\archive\";
  var rpaExtractor = new RpaExtractor();
  rpaExtractor.Extract(rpaFile, outputDir);
```

## Download

Compiled downloads are not available.

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiExtrpact

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

iiExtrpact is licenced under the MIT License. Full licence details are available in licence.md