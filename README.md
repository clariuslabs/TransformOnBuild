![Icon](https://raw.github.com/clariuslabs/TransformOnBuild/master/icon/32.png) Transform Text Templates On Build
============

Automatically transforms on build all files with a build action of `None` that have the `TextTemplatingFilePreprocessor` or `TextTemplatingFileGenerator` custom tools associated.

## Installation

To install Clarius Transform Text Templates On Build, run the following command in the Package Manager Console:

```
PM> Install-Package Clarius.TransformOnBuild
```

Unlike the [officially suggested way](http://msdn.microsoft.com/en-us/library/ee847423.aspx), this package does not require any Visual Studio SDK to be installed on the machine or build server.
