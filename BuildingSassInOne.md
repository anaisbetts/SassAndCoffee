# How to build Sass-In-One

1. Check out Sass.
2. Copy the ext/build\_sass\_in\_one.rb script to the sass/lib directory.
3. ruby build\_sass\_in\_one.rb > sass\_in\_one.rb 2>log.txt - this will
   probably throw a few errors.
4. Keep trying to run sass\_in\_one.rb and fixing up the runtime errors 
   (Sass tries to build its version info from files in the Gem - replace
    it with hardcoded strings)
