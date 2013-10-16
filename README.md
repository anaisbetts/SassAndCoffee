# SassAndCoffee
This library adds simple, drop-in support for Sass/SCSS as well as CoffeeScript.
Javascript and CoffeeScript files can also be minified and combined via UglifyJS.

## How to use:
First, add the desired [package reference(s)](#Packages) via [NuGet]. You probably want SassAndCoffee.AspNet.

### CoffeeScript
* Add a .coffee file to your project. For purposes of this example we'll use ~/Scripts/main.coffee
* Reference that file in your page as if it were compiled JavaScript: `<script src="/Scripts/main.js" />`
* By default the CoffeeScript compiler will wrap your scripts.  If for some reason you want bare mode, request it: `<script src="/Scripts/main.bare.js" />`
* *Note*: If a file exists named ~/Scripts/main.js, it will be served instead of ~/Scripts/main.coffee.  SassAndCoffee *always* preferentially serves existing files on disk.

### JavaScript Minification
* You can request minification of *any* JavaScript file by adding .min to the filename before .js.
    `<script src="/Scripts/main.min.js" />` will serve the minified version of `<script src="/Scripts/main.js" />`
* This of course works with CoffeeScript files (as described above), including bare support: `<script src="/Scripts/main.bare.min.js" />`

### JavaScript Combination
* Create a .combine file.  We'll use ~/Scripts/home.combine for this example.
* Fill it with the absolute, relative, and App-relative paths of the scripts you want included:

```
# This line is a comment
# Include our minified CoffeeScript file with an absolute path
~/Scripts/main.min.js
# Include another file in ~/Scripts
header.js
# Include a file in a subfolder of ~/Scripts
subfolder/file.js
```

* Reference that file in your page as if it were compiled JavaScript: `<script src="/Scripts/home.js" />`
* *Note*: For now, we recommend that you minify the included files, rather than the combined file.  Uglify can choke on exceedingly large inputs.
* *Note*: If a file exists named ~/Scripts/home.js, it will be served instead of ~/Scripts/home.combine.  SassAndCoffee *always* preferentially serves existing files on disk.

### Sass/SCSS
* Add a .scss or .sass file to your project. We'll use ~/Content/site.scss for this example.
* Reference that file in your page as if it were compiled CSS: `<link href="/Content/site.css" type="text/css" />`
* CSS combination can be achieved with `@import "";` directives
* Minification works just like with JavaScript (but only for Sass/SCSS files). Just add .min to the filename before .css.
    `<link rel="stylesheet" href="/Content/site.min.css" type="text/css" />` will serve the minified version of `<link rel="stylesheet" href="/Content/site.css" type="text/css" />`
* *Note*: If a file exists named ~/Content/site.css, it will be served instead of ~/Scripts/site.scss.  SassAndCoffee *always* preferentially serves existing files on disk.

## What about performance?
SassAndCoffee offloads caching and compression to ASP.Net and IIS. To modify the default cache settings, alter the `SassAndCoffeeCacheSettings` cache profile in web.config.  To enable compression, install IIS's compression features.

SassAndCoffee watches your source files for changes and invalidates the cache when appropriate.

## How does it work?
SassAndCoffee embeds the original compilers in the DLL (Sass 3.2.0 and CoffeeScript 1.1.3
as of this writing) and uses IronRuby and JScript respectively to execute the
compilers against your source.

SassAndCoffee will even use IE9's faster Chakra JScript engine if it's detected.  Since JScript doesn't ship with Windows Server Core editions, it will not work on those systems.

## Why is this better than [SOMEOTHERPROJECT]?
* No external processes are executed
* You don't have to install Ruby or node.js
* It's in NuGet so you don't have to fiddle with web.config
* Files are cached and are rebuilt as-needed.

## Problems
If you run into bugs / have feature suggestions / have questions, please either send me an Email at paul@paulbetts.org, or file a Github bug. 

## Thanks to:
Several folks helped me out with some of the integration details of this project
- if it weren't for them, I would still be stuck in the mud right now:

* David Padbury for helping me out with the CoffeeScript compiler
* Levi Broderick for giving me a few hints as to how to rig up the HttpModule
* Jimmy Schementi for telling me the proper way to redirect 'requires' to an embedded resource
* Thanks to Hampton Catlin and Jeremy Ashkenas for creating such awesome languages in the first place
* The folks on the #chromium IRC channel for helping me with a tricky V8 issue
* Steven Robbins for the ton of work put in to refactor the code to work with NancyFx and other non-ASP.NET frameworks

## <a name="Packages" /> Packages
### SassAndCoffee.AspNet
Adds Sass/SCSS, CoffeeScript, and UglifyJS support to ASP.Net projects.  Works with WebForms and MVC

### SassAndCoffee.JavaScript
The SassAndCoffee JavaScript compilers for CoffeeScript and UglifyJS. Great for integration into your own packages.  Just include it as a dependency.

### SassAndCoffee.Ruby
The SassAndCoffee Ruby compilers for Sass/SCSS. Great for integration into your own packages.  Just include it as a dependency.

### SassAndCoffee.Core
Shared components used by SassAndCoffee.JavaScript and SassAndCoffee.Ruby. Unlikely you'll want to reference this directly.

### SassAndCoffee
This legacy package is now an alias for SassAndCoffee.AspNet.

<a name="Links" />

[Nuget]: http://nuget.org/ (Nuget)
