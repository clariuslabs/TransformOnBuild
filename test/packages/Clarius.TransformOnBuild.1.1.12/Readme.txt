Usage:

    Simply continue using `TextTemplatingFilePreprocessor` or `TextTemplatingFileGenerator` custom 
    tools associated with your .tt/.t4 files as usual. This package will add a .targets file that 
    will also transform them on build, without requiring the installation of any Visual Studio SDK.


Release Notes:

v1.1

* Disabled processing of T4 files with 'TextTemplatingFilePreprocessor' since they aren't supported by TextTemplating.exe according to http://stackoverflow.com/a/9198532.

v1.0

* Automatically transform on build all None files with one of the T4 custom tools assigned.
