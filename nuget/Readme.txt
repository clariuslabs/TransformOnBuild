Usage:

    Simply continue using `TextTemplatingFileGenerator` custom 
    tools associated with your .tt/.t4 files as usual. This package will add a .targets file that 
    will also transform them on build, without requiring the installation of any Visual Studio SDK.

Release Notes:

v1.22

* Fixed support for Visual Studio 2017 v15.8 - possible regression with Visual Studio dll lock file

* Add support for Visual Studio 2017 BuildTools

* Don't clutter project folder with temporary backup files

v1.21

* Merged NuGet Package Clarius.TransformOnBuild-unofficial

* Add support for Visual Studio 2017 and Preview

* Add support for passing transform parameters

* Add support for Visual Studio 2015 Update 1

* Fix path with spaces

* Transform items with Build Action = Content

* Ensure ResolveReferences and _CopyFilesMarkedCopyLocal will run during the main build

* Transform before build and manually trigger ResolveReferences

* Copy custom task assembly to the temp folder to prevent locking

* Copy Local dlls to TargetDir before transformation to allow use of $(TargetDir)

* Substitute MSBuild variables such as $(ProjectDir)

v1.1

* Disabled processing of T4 files with 'TextTemplatingFilePreprocessor' since they aren't supported by TextTemplating.exe according to http://stackoverflow.com/a/9198532.

v1.0

* Automatically transform on build all None files with one of the T4 custom tools assigned.
