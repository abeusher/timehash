/*
This module creates a fuzzy precision representation of a time interval.
- It makes calculations based on a 64 year representation of time from January 1, 1970 to January 1, 2098.
- Values are encoded with a number of bits(represented by an ASCII character) that indicate the amount of time to add to 1970.
- Times prior to 1970 or after 2098 are not accounted for by this scale.
- Each character added to the timehash reduces the time interval ambiguity by a factor of 8.
- Valid characters for encoding the floating point time into ASCII characters include {01abcdef}

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
10 +/- 1.88097 seconds
*/

package main

import (
	"fmt"
	"strconv"
	"strings"
	"time"
)

const _base32 string = "01abcdef"
const mapBefore string = "f01abcde"
const mapAfter string = "1abcdef0"
const lowerBound float64 = 0.0
const upperBound float64 = 4039372800.0
const defaultPrecision int = 10

var bits = [3]int{4, 2, 1}
var decodeMap = make(map[string]int)
var neighborMap = make(map[string]Neighbor)

// Neighbor holds the timehash before, during, and after
// the given window to exclude the given timehash.
type Neighbor struct {
	before string
	after  string
}

// Neighborhood holds the timehash before, during, and after
// the given window to include the given timehash.
type Neighborhood struct {
	before string
	center string
	after  string
}

func main() {
	initMaps()
	examples()
}

func examples() {
	testHash := "af1cef0" // '2016-05-27T01:55:57.202148'
	th, slop := DecodeExactly(testHash)
	ths := strconv.FormatFloat(th, 'f', 7, 64)
	slops := strconv.FormatFloat(slop, 'f', 7, 64)
	fmt.Print("Encoded with margin ", ths, slops, "\n")
	fmt.Print("Encoded ", Encode(1516933969.398167, defaultPrecision), "\n")
	fmt.Print("Before ", Before(testHash), "\n")
	fmt.Print("After: ", After(testHash), "\n")
	fmt.Print("Encode from datetime ", EncodeDate(time.Now()), "\n")
}

func initMaps() {
	for i := range _base32 {
		decodeMap[string(_base32[i])] = i
		neighborMap[string(_base32[i])] = Neighbor{string(mapBefore[i]), string(mapAfter[i])}
	}
	fmt.Println(decodeMap)
}

// DecodeExactly decodes a timehash and get the margin of error
// on that calculation in seconds.
func DecodeExactly(timehash string) (float64, float64) {
	lower, upper := lowerBound, upperBound
	var mid float64
	timeError := (lower + upper) / 2

	for _, c := range timehash {
		dc := decodeMap[string(c)]
		for _, mask := range bits {
			timeError = timeError / 2
			mid = (lower + upper) / 2

			if mask&dc > 0 {
				lower = mid
			} else {
				upper = mid
			}
		}
	}

	value := (lower + upper) / 2
	return value, timeError
}

// Decode the geohash but throw away the margin of error.
func Decode(timehash string) float64 {
	epochSeconds, _ := DecodeExactly(timehash)
	return epochSeconds
}

// Encode the given seconds since the epoch into a timehash
// at the given precision.
func Encode(epochSeconds float64, precision int) string {
	// If no precision offered (zero value), default to 10.
	if precision == 0 {
		precision = defaultPrecision
	}
	var timehash []string
	var ch, bit int
	lower, upper := lowerBound, upperBound

	for len(timehash) < precision {
		mid := (lower + upper) / 2
		if epochSeconds > mid {
			ch = ch | bits[bit]
			lower = mid
		} else {
			upper = mid
		}
		if bit < 2 {
			bit = bit + 1
		} else {
			timehash = append(timehash, string(_base32[ch]))
			bit = 0
			ch = 0
		}
	}

	return strings.Join(timehash, "")
}

// Before determines the timehash for the window directly
// preceding the given timehash at the same precision.
func Before(timehash string) string {
	i := 1
	var value string

	for j := len(timehash) - 1; j >= 0; j-- {
		c := string(timehash[j])
		if c != "0" {
			padding := strings.Repeat("f", i-1)
			position := len(timehash) - i
			value = timehash[0:position] + neighborMap[c].before + padding
			break
		} else {
			i = i + 1
		}
	}

	return value
}

// After determines the timehash for the window directly
// after the given timehash at the same precision.
func After(timehash string) string {
	i := 1
	var value string

	for j := len(timehash) - 1; j >= 0; j-- {
		c := string(timehash[j])
		if c != "f" {
			padding := strings.Repeat("0", i-1)
			position := len(timehash) - i
			value = timehash[0:position] + neighborMap[c].after + padding
			break
		} else {
			i = i + 1
		}
	}

	return value
}

// Neighbors returns the neighboring windows to either side
// of the provided timehash at the same precision.
func Neighbors(timehash string) Neighbor {
	return Neighbor{Before(timehash), After(timehash)}
}

// Expand the given timehash window to include the preceding
// and following window, including the given timehash.
func Expand(timehash string) Neighborhood {
	return Neighborhood{Before(timehash), timehash, After(timehash)}
}

// EncodeDate takes a Time object and turns it into a
// timehash at the default precision.
func EncodeDate(date time.Time) string {
	return Encode(float64(date.Unix()), defaultPrecision)
}
