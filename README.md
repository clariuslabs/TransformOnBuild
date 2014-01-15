# ![Icon](https://raw.github.com/clariuslabs/TransformOnBuild/master/icon/32.png) Transform Text Templates On Build [![Build Statis](https://www.myget.org/BuildSource/Badge/clarius?identifier=6f42d5d3-ec58-4246-91a7-8722dbb023ea)](https://www.myget.org/gallery/clarius)

============

Automatically transforms on build all files with a build action of `None` that have the `TextTemplatingFilePreprocessor` or `TextTemplatingFileGenerator` custom tools associated.

## Installation

To install Clarius Transform Text Templates On Build, run the following command in the Package Manager Console:

```
PM> Install-Package Clarius.TransformOnBuild
```

Unlike the [officially suggested way](http://msdn.microsoft.com/en-us/library/ee847423.aspx), this package does not require any Visual Studio SDK to be installed on the machine or build server.

If a full Visual Studio installation is not available on the build server, you can still transform the templates by placing the TextTransform.exe in a known location. Then, you can simply override the path expected by the targets with:

	<PropertyGroup>
		<TextTransformPath>MyTools\TextTransform.exe</TextTransformPath>
	</PropertyGroup>


With that in place, the transformation will be performed using that file instead, if found.
