timehash
========

About
-----
timehash is an algorithm (with multiple reference implementations) for calculating variable precision sliding windows of time.
When performing aggregations and correlations on large-scale data sets, the ability to convert precise time values into 'malleable intervals' allows for many novel analytics.

Using [sliding windows of time](http://stackoverflow.com/questions/19386576/sliding-window-over-time-data-structure-and-garbage-collection) is a common practice in data analysis but prior to the timehash algorithm it was more of an art than a science.

Features
--------
* convert epoch miliseconds into an interval of time, depicted by an ASCII character 'hash' (a 'timehash')
* timehash values are well suited to referencing time intervals in key-value stores (e.g. Hbase, Acculumo, Redis)
* The creation of a compound key of space and time (e.g. geohash_timehash) is a powerful primitive for understanding geotemporal patterns

Usage
-----
Calculate a timehash value
```python
import timehash
rightnow = time.time()
rightnow_hash = encode(rightnow, precision=10)
print rightnow_hash
```

License
-------
[Modified BSD License][http://en.wikipedia.org/wiki/BSD_licenses#3-clause_license_.28.22Revised_BSD_License.22.2C_.22New_BSD_License.22.2C_or_.22Modified_BSD_License.22.29] (3-clause license)

Contact
-------
[AbeUsher](http://www.linkedin.com/in/socialnetworkanalysis)
