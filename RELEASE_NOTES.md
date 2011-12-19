# Release Notes
## v2.0
v2.0 is a major relase and contains potentially breaking changes. Please carefully read these release notes and thoroughly test your app before upgrading.

### Breaking Changes
* Assembly structure and code layout dramatically changed.  Not API compatible *at all* with previous versions.
* CoffeeScript is now wrapped by default (matches CoffeeScript compiler). To request bare versions use .bare.js.
* Caching behavior dramatically changed. Ensure IIS is configured correctly or performance will be abysmal.
* Dropped support for Server Core editions.
* Full Trust is now officially required (might have been before, not sure).

### Core Changes
* Switched from V8 to Window's built in JScript engine. Simplifies deployment on diverse Windows versions and x86/x64.
* Moved caching and compression to IIS/ASP.Net
    * To modify the default cache settings, alter the `SassAndCoffeeCacheSettings` cache profile in web.config.
    * To enable compression, follow the directions for [IIS 6] [IIS6Compression] or [IIS 7/7.5] [IIS7Compression].

### Enhancements
* Upgraded to CoffeeScript 1.1.3
* Support for bare (.bare.js) and wrapped (.js) CoffeeScript

### Bug Fixes
* General - Cache settings are now configurable in web.config (#34)
* General - Move caching and compression to IIS/ASP.NET (#37)
* CoffeeScript - Support wrapped and bare mode (#27)
* CoffeeScript - Existing files are served by the StaticFileHandler in IIS (#32)
* Combine - .combine now inserts ';' between combined files (#5 #15)
* Combine - Switch to Windows' JScript engine (#18 #21 #23 #36)
* Sass/SCSS - Existing files are served by the StaticFileHandler in IIS (#10)
* Sass/SCSS - Imported files are tracked for cache invalidation (#12)

[IIS6Compression]: http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/d52ff289-94d3-4085-bc4e-24eb4f312e0e.mspx?mfr=true "IIS 6 Compression Settings"
[IIS7Compression]: http://technet.microsoft.com/en-us/library/cc771003(v=ws.10).aspx "IIS 7 Compression Settings"

## Prior versions
Release notes for these versions are not available. Please see the commit log instead.