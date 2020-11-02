![Icon](https://raw.github.com/clariuslabs/TransformOnBuild/master/icon/32.png) Transform Text Templates On Build
============

> NOTE: this repository is no longer actively maintained. T4 has long been surpassed by far more capable 
> templating alternatives (i.e. Scriban and may others) and there's even an open source [Mono implementation](https://www.nuget.org/packages/Mono.TextTemplating/) 
> which is far faster that the (effectively legacy) T4 in VS. Running T4 templates as part of the build 
> did never seem to be a core design or intended usage, and it's consequently quite painful and slow in general.
> Switching CI and updating the project to use newer techniques and fix the many reported issues is something 
> that just didn't make sense anymore.
> Feel free to fork and do anything you want with it, that's the beauty of open source :).
> If you want to volunteer as a maintainer, please let us know at https://github.com/clariuslabs/TransformOnBuild/issues/68

Automatically transforms on build all files with a build action of `None` or `Content` that have the `TextTemplatingFileGenerator` or `TransformOnBuild` custom tools associated.

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

If you would like to pass parameters to TextTransform.exe, define a group of TextTransformParameter items as follows:

    <ItemGroup>
        <TextTransformParameter Include="Foo">
            <Value>bar</Value>
            <InProject>false</InProject>
        </TextTransformParameter>
        <TextTransformParameter Include="Config">
            <Value>$(Configuration)</Value>
            <InProject>false</InProject>
        </TextTransformParameter>
    </ItemGroup>


The Include attribute specifies the parameter name, and the Value metadata element specifies the parameter value.

To access the parameter values from your text template, set `hostspecific` in the `template` directive and invoke `this.Host.ResolveParameterValue(...)`. For example:

    <#@ template language="C#" hostspecific="true" #>
    <#
        var foo = this.Host.ResolveParameterValue("", "", "Foo");
        var config = this.Host.ResolveParameterValue("", "", "Config");
    #>
