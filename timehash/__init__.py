#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
    timehash.py - A library by Abe Usher to help compute variable precision time intervals,
    for use in Big Data analysis, spatial-temporal computation, and other quantitative data analysis.
    Copyright (C) 2010  Abe Usher abe.usher@gmail.com

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:
        * Redistributions of source code must retain the above copyright
          notice, this list of conditions and the following disclaimer.
        * Redistributions in binary form must reproduce the above copyright
          notice, this list of conditions and the following disclaimer in the
          documentation and/or other materials provided with the distribution.
        * Neither the name of Abe Usher nor the
          names of its contributors may be used to endorse or promote products
          derived from this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
    ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
    WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL ABE USHER BE LIABLE FOR ANY
    DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
    (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
    LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
    ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
    (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
"""
import time
import datetime

__base32 = '01abcdef'
__before = 'f01abcde'
__after = '1abcdef0'
__decodemap = { }
__neighbormap = { }

for i in range(len(__base32)):
    __decodemap[__base32[i]] = i
    __neighbormap[__base32[i]] = (__before[i], __after[i])
del i

"""
This module creates a fuzzy precision representation of a time interval.

It makes calculations based on a 64 year representation of time from January 1, 1970 to January 1, 2098.
Values are encoded with a number of bits(represented by an ASCII character) that indicate the amount of time to add to 1970.
Times prior to 1970 or after 2098 are not accounted for by this scale.

Each character added to the timehash reduces the time interval ambiguity by a factor of 8.
Valid characters for encoding the floating point time into ASCII characters include {01abcdef}

0 +/- 64 years
1 +/- 8 years
2 +/- 1 years
3 +/- 45.65625 days
4 +/- 5.707 days
5 +/- 0.71337 days = 17.121 hours
6 +/- 2.14013671875 hours
7 +/- 0.26751708984375 hours = 16.05 minutes
8 +/- 2.006378173828125 minutes
9 +/- 0.2507 minutes = 15 seconds
10 +/- 1.88097 seconds"""


def decode_exactly(timehash):
    """
    Decode the timehash to its exact values, including the error
    margins of the result.  Returns two float values: timehash
    and the plus/minus error for epoch seconds.
    """
    time_interval = (0.0, 4039372800.0)#from January 1, 1970 to January 1, 2098
    time_error = (time_interval[0] + time_interval[1])/2  #this constant is used to calculate the potential time error defined by a particular number of characters
    for c in timehash:
        cd = __decodemap[c]
        for mask in [4, 2, 1]:
            time_error /=2
            mid = (time_interval[0] + time_interval[1])/2
            if cd & mask:
                time_interval = (mid, time_interval[1])
            else:
                time_interval = (time_interval[0], mid)
    time_value = (time_interval[0] + time_interval[1])/2
    return (time_value, time_error)

def decode(timehash):
    """
    Decode timehash, returning a single floating point value for epoch seconds.
    """
    epoch_seconds, time_error = decode_exactly(timehash)
    #drop the time_error for now
    return epoch_seconds

def encode(timeseconds, precision=10):
    """
    Encode a timestamp given as a floating point epoch time to
    a timehash which will have the character count precision.
    """
    time_interval = (18000.0, 4039372800.0)#from January 1, 1970 to January 1, 2098
    timehash = []
    bits = [4, 2, 1]
    bit = 0
    ch = 0
    while len(timehash) < precision:
        mid = (time_interval[0] + time_interval[1])/2
        if timeseconds > mid:
            ch |= bits[bit]
            time_interval = (mid, time_interval[1])
        else:
            time_interval = (time_interval[0], mid)
        if bit < 2:
            bit += 1
        else:
            timehash += __base32[ch]
            bit = 0
            ch = 0
    return ''.join(timehash)

def before(hashcode):
    """
    Extract the hashcode for the preceding time-window.
    """
    i = 1
    for c in reversed(hashcode):
        padding = (i - 1) * 'f'
        pos = len(hashcode) - i
        if c != '0':
            ret = hashcode[:pos] + __neighbormap[c][0] + padding
            return ret
        else:
            i += 1

def after(hashcode):
    """
    Extract the hashcode for the succeeding time-window.
    """
    i = 1
    for c in reversed(hashcode):
        padding = (i - 1) * '0'
        pos = len(hashcode) - i
        if c != 'f':
            ret = hashcode[:pos] + __neighbormap[c][1] + padding
            return ret
        else:
            i += 1

def neighbors(hashcode):
    """
    Extract the hashcodes for the preceding and succeeding time-windows,
    excluding the hashcode for the current time-window.
    """
    return [before(hashcode), after(hashcode)]

def expand(hashcode):
    """
    Extract the hashcodes for the preceding and succeeding time-windows,
    including the hashcode for the current time-window.
    """
    return [before(hashcode), hashcode, after(hashcode)]


if __name__ == "__main__":
    """
    Main function - entry point into script.  Used for examples and testing.
    """

    # Examples of encoding
    rightnow = time.time()
    rightnow_hash = encode(rightnow, precision=10)

    rightnow60 = rightnow + 60
    rightnow60_hash = encode(rightnow60)

    rightnow3600 = rightnow + 3600
    rightnow3600_hash = encode(rightnow3600)

    previous3600 = rightnow - 3600
    previous3600_hash = encode(previous3600)

    previous60 = rightnow - 60
    previous60_hash = encode(previous60)

    previous86400 = rightnow - 86400
    previous86400_hash = encode(previous86400)

    year_future_from_now = rightnow + (86400*365.25)
    year_future_from_now_hash = encode(year_future_from_now)

    print("one day ago\t\t{}".format(previous86400_hash))
    print("one hour ago\t\t{}".format(previous3600_hash))
    print("60 seconds ago\t\t{}".format(previous60_hash))
    print("now\t\t\t{} ***".format(rightnow_hash))
    print("60 seconds future\t{}".format(rightnow60_hash))
    print("one hour in the future\t{}".format(rightnow3600_hash))
    print("one year from today\t{}".format(year_future_from_now_hash))
    print("\n")

    # Example of decoding and error check
    rightnow_calculated,time_error = decode_exactly(rightnow_hash)
    print("original rightnow = %.2f (%s)" % (rightnow,rightnow_hash))
    print("calculated rightnow = %.2f" % (rightnow_calculated))
    print("time error = +/- %.2f seconds." % (time_error))    #

    print("\n")
    print("\n")
    today = 'add0c'
    rightnow_calculated,time_error = decode_exactly(today)
    dt = datetime.datetime.fromtimestamp(rightnow_calculated)
    print(dt)
    print("original rightnow = %.2f (%s)" % (rightnow,rightnow_hash))
    print("calculated rightnow = %.2f" % (rightnow_calculated))
    print("time error = +/- %.2f seconds." % (time_error))
