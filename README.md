WMI Crawler
===========

WMI Crawler is a simple command like application that crawls the WMI Namespaces, Classes, and Class Properties on the current system.  It is rather simplistic in its output however it is very easy to parse the output to find a namespace or a class you are looking for.

###Command Line Usage

```AsciiDoc
wmicrawler [options]

Crawls only the namespaces if no options are provided.

Options:

  -sc or --show-classes                Crawls the classes within each namespace.
  -sp or --show-properties             Crawls the properties for each class, works only with --show-classes.
  -sq or --show-property-qualifiers    Crawls the qualifiers for each property, works only with --show-properties.
