![51Degrees](https://51degrees.com/DesktopModules/FiftyOne/Distributor/Logo.ashx?utm_source=github&utm_medium=repository&utm_content=home&utm_campaign=net-open-source "THE Fastest and Most Accurate Device Detection") **Device Detection for Microsoft .NET**

[Recent Changes](#recent-changes "Review recent major changes") | [Supported Databases](https://51degrees.com/compare-data-options?utm_source=github&utm_medium=repository&utm_content=home-menu&utm_campaign=net-open-source "Different device databases which can be used with 51Degrees device detection") | [.NET Developer Documention](https://51degrees.com/support/documentation/net?utm_source=github&utm_medium=repository&utm_content=home-menu&utm_campaign=net-open-source "Full getting started guide and advanced developer documentation") | [Available Properties](https://51degrees.com/resources/property-dictionary?utm_source=github&utm_medium=repository&utm_content=home-menu&utm_campaign=net-open-source "View all available properties and values")

<sup>Need [C](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for C") | [Java](https://github.com/51Degrees/Java-Device-Detection "THE Fastest and most Accurate device detection for Java") | [PHP](https://github.com/51Degrees/Device-Detection) | [Python](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for Python") | [Perl](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for Perl") | [Node.js](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for Node.js")?</sup>

**Server Side:** Use code like...

```cs
Request.Browser["IsMobile"]
```

or 

```cs
Request.Browser["IsTablet"]
```

... from within a web application server side to determine the requesting device type.

**Client Side:** Include...

```
https://[YOUR DOMAIN]/51Degrees.features.js?DeviceType&ScreenInchesDiagonal
```

... from Javascript to retrieve device type and physcial screen size information. Use Google Analytics custom dimensions to add this data for more granular analysis.

**Offline:** Use...

```cs
var detectionProvider = new Provider(StreamFactory.Create("[DATA FILE LOCATION]"));
var deviceType = detectionProvider.Match("[YOUR USERAGENT]")["DeviceType"];
```

... to perform offline analysis of web logs with User-Agent headers.

**[Review All Properties](https://51degrees.com/resources/property-dictionary?utm_source=github&utm_medium=repository&utm_content=home-cta&utm_campaign=net-open-source "View all available properties and values")**

## What's needed?

The simplest method of deploying 51Degrees device detection to a .NET project is with NuGet. Just search for [51Degrees on NuGet](https://www.nuget.org/packages?q=51degrees "51Degrees Packages on NuGet").

This GitHub repository and NuGet include 51Degrees free Lite device database. The Lite data is updated monthly by our professional team of analysts. 

Data files which are updated weekly and daily, automatically, and with more properties and device combinationsare also available.

**[Compare Device Databases](https://51degrees.com/compare-data-options?utm_source=github&utm_medium=repository&utm_content=home-cta&utm_campaign=net-open-source "Compare different data file options for 51Degrees device detection")**

## Recent Changes

### Version 3.2.12 Highlights

New Lite Data File released for September.

### Major Changes in Version 3.2

* Embedded data has been removed from the assembly and now must be provided from the App_Data folder.
* .NET 3.5 is not supported in this release in order to use memory mapped files and simplify overriding default browser capabilities.
* In stream mode entity data properties that can allocate large arrays only initialise these arrays when needed.
* Caches used with stream operation are now fixed memory size and serviced via the thread pool.
* Automatic update processes uses temporary files rather than main memory to verify integrity of updated files prior to using them.
* Temporary files are now created in the App_Data/51Degrees folder of the web application rather than a UNC path or the master data file folder.
* Values associated with Profiles are now retrieved using a more efficient algorithm.
* DataSet.Properties collection now has a string accesser to make retrieving properties by name simpler.
* Web sites using memory mode use a byte array to improve start up time.
* Version 3.2 data file formats are supported in parallel with version 3.1 data files.
* 51Degrees unit tests are now part of the open source distribution.

### Changes from 3.2.11

Summary of API changes:

* When the WebProvider is being used outside a web environment to make use of the update feature, it is useful to see the return code of the data file download. The codes are detailed in the LicenceKeyResults enumeration. Updating manually in this way may mean that the Download method is called when there is no bin directory (where the API searches for a licence key file). For this reason a check has been added to the Keys method in LicenceKeys which checks for the existance of the bin directory before searching it for a licence key file.
* Added all profiles example, and removed unnecessary configurations.
* Updated the Lite data files for September data.