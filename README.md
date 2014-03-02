WMI Crawler
===========

WMI Crawler is a simple command line application that crawls for WMI Namespaces, Classes, and Class Properties on the current system.  It is rather simplistic in its output and is easy to parse the output to find a namespace or a class you are looking for by piping the output to another command or redirecting the output to a text file.

###Command Line Usage

```AsciiDoc
wmicrawler [options]

Crawls only the namespaces if no options are provided.

Options:

  -sc or --show-classes                Crawls the classes within each namespace.
  -sp or --show-properties             Crawls the properties for each class, works only with --show-classes.
  -sq or --show-property-qualifiers    Crawls the qualifiers for each property, works only with --show-properties.
