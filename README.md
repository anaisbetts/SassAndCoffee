# SassAndCoffee

This library adds simple, drop-in support for Sass/SCSS as well as Coffeescript.

How to use:

* Add the package reference via NuGet
* Add a .coffee, .scss, or .sass file to your project (an easy test is to just
  rename a CSS file to .scss)
* Reference the file as if it was a CSS file (i.e. to reference
  "scripts/test.coffee", you should reference "scripts/test.js" in your SCRIPT
  tag)

That's all there is to it! Files will be cached in your AppData folder and will
be regenerated whenever you modify them.


# How does it work?

SassAndCoffee embeds the original compilers in the DLL as (Sass 3.2.0 and CoffeeScript 1.1.0
as of this writing) and uses IronRuby and Jurassic respectively to execute the
compilers against your source.


# Why is this better than [SOMEOTHERPROJECT]

* No external processes are executed
* You don't have to install Ruby or node.js
* It's in NuGet so you don't have to fiddle with web.config
* Files are cached and are rebuilt as-needed.


# Thanks to:

Several folks helped me out with some of the integration details of this project
- if it weren't for them, I would still be stuck in the mud right now:

* David Padbury for helping me out with the CoffeeScript compiler
* Levi Broderick for giving me a few hints as to how to rig up the HttpModule
* Jimmy Schementi for telling me the proper way to redirect 'requires' to an embedded resource
