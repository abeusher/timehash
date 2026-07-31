

# TimeHash

TimeHash is a library for encoding and decoding timestamps into variable-precision time intervals. It creates a fuzzy, hierarchical representation of time, making it ideal for Big Data analysis, spatial-temporal computations, and efficient time-series indexing.

By encoding Unix epoch seconds into a compact string using a custom alphabet (`01abcdef`), TimeHash allows you to quickly determine temporal proximity, find neighboring time windows, and adjust precision on the fly without complex datetime arithmetic.

## Time Range & Precision
TimeHash covers a 128-year interval from **January 1, 1970** to **January 1, 2098**. Each character added to the hash reduces the time interval ambiguity by a factor of 8.

| Length | Approximate Precision (±) |
|:------:|:--------------------------|
| 0      | 64 years                  |
| 1      | 8 years                   |
| 2      | 1 year                    |
| 3      | 45.6 days                 |
| 4      | 5.7 days                  |
| 5      | 17.1 hours                |
| 6      | 2.14 hours                |
| 7      | 16.05 minutes             |
| 8      | 2.01 minutes              |
| 9      | 15.04 seconds             |
| 10     | 1.88 seconds              |

## Supported Languages
TimeHash has been implemented in multiple languages:
- Python (`timehash` package)
- C (`c-impl/`)
- C++ (`time_hash.h`)
- C# / .NET (`TimeHash.cs`)
- Go (`timehash.go`)
- Java (`TimeHash.java`)
- Perl (referenced in project history)

## Installation (Python)
Install the Python package via `pip`:
```bash
pip install timehash
```
Or install from source:
```bash
git clone https://github.com/abeusher/timehash.git
cd timehash
pip install .
```

## Usage (Python)
```python
from timehash import encode, decode, decode_exactly, before, after, neighbors, expand
from datetime import datetime
import time

# Encode the current time with 10 characters of precision
current_time = time.time()
th = encode(current_time, precision=10)
print(f"TimeHash: {th}")

# Decode back to epoch seconds
epoch = decode(th)
print(f"Decoded epoch: {epoch}")

# Decode with margin of error
center, error = decode_exactly(th)
print(f"Center: {center} ± {error} seconds")

# Get neighboring time windows at the same precision
prev_th = before(th)
next_th = after(th)
print(f"Previous: {prev_th} | Next: {next_th}")

# Get all three (before, current, after)
window = expand(th)
print(f"Time window: {window}")

# Encode directly from a datetime object
dt = datetime(2023, 10, 25, 14, 30, 0)
dt_th = encode_from_datetime(dt, precision=8)
print(f"Datetime TimeHash: {dt_th}")
```

## API Overview
| Function | Description |
|----------|-------------|
| `encode(epoch_time, precision)` | Encodes a floating-point Unix timestamp into a TimeHash string. |
| `decode(timehash)` | Decodes a TimeHash string back to a floating-point Unix timestamp. |
| `decode_exactly(timehash)` | Returns `(center, error)` representing the window midpoint and half-margin in seconds. |
| `before(timehash)` / `after(timehash)` | Returns the adjacent TimeHash strings at the same precision level. |
| `neighbors(timehash)` | Returns `[before, after]` list. |
| `expand(timehash)` | Returns `[before, current, after]` list. |
| `encode_from_datetime(datetime_obj, precision)` | Converts a Python `datetime` object directly to a TimeHash. |

## License
TimeHash is released under the **BSD 3-Clause License**. See the source files for full copyright and disclaimer information.

## Contributors & History
Created by **Abe Usher** in 2010. TimeHash has been improved through feedback and contributions from Dan Noble, Danny Holloway, Michele Bueno, David Sherman, Kevin Dwyer, Michele Casey (Java), and Altaf Bahora (Perl). A full history of changes is available in [CHANGES.md](CHANGES.md).
