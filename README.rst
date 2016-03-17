timehash
========

About
-----

timehash is an algorithm (with multiple reference implementations) for
calculating variable precision sliding windows of time. When performing
aggregations and correlations on large-scale data sets, the ability to
convert precise time values into 'malleable intervals' allows for many
novel analytics.

Using `sliding windows of
time <http://stackoverflow.com/questions/19386576/sliding-window-over-time-data-structure-and-garbage-collection>`__
is a common practice in data analysis but prior to the timehash
algorithm it was more of an art than a science.

Features
--------

-  convert epoch miliseconds into an interval of time, depicted by an
   ASCII character 'hash' (a 'timehash')
-  timehash values are well suited to referencing time intervals in
   key-value stores (e.g. Hbase, Acculumo, Redis)
-  The creation of a compound key of space and time (e.g.
   geohash\_timehash) is a powerful primitive for understanding
   geotemporal patterns

Implementations
---------------

-  `python
   timehash <https://github.com/abeusher/timehash/blob/master/timehash/__init__.py>`__
   - a reference implementation in pure python
-  `perl
   timehash <https://github.com/abeusher/timehash/blob/master/timehash.pl>`__
   - a reference implementation in perl
-  `java
   timehash <https://github.com/abeusher/timehash/blob/master/TimeHash.java>`__
   - a reference implementation in java

Usage
-----

Example of calculating a timehash value in python:

.. code:: python

    import timehash
    import time

    rightnow = time.time()
    rightnow60 = rightnow + 60.0

    rightnow_hash = timehash.encode(rightnow, precision=10)
    rightnow60_hash = timehash.encode(rightnow60, precision=10)

    print 'timehash of right now: %s' % rightnow_hash
    print 'timehash of now +60s: %s'% rightnow60_hash
     
    % timehash of right now: ae0f0ba1fc
    % timehash of now +60s: ae0f0baa1c

License
-------

`Modified BSD
License <http://en.wikipedia.org/wiki/BSD_licenses#3-clause_license_.28.22Revised_BSD_License.22.2C_.22New_BSD_License.22.2C_or_.22Modified_BSD_License.22.29>`__

Contact
-------

- TimeHash Guru: `AbeUsher <http://www.linkedin.com/in/socialnetworkanalysis>`__
- Python Packager: `Kevin Dwyer / @pheared <https://twitter.com/pheared>`__

