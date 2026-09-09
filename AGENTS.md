# AGENTS.md — AI Agent Guide for `timehash`

This document provides architectural context, algorithm specifications, repository layout, and development guidelines for AI agents working within this repository.

---

## 1. Overview & Purpose

`timehash` is an algorithm and collection of reference implementations created by Abe Usher for calculating variable-precision, sliding-window temporal hashes. Analogous to geohash in spatial computing, `timehash` maps continuous timestamps (epoch seconds/milliseconds) into deterministic, hierarchical ASCII strings.

Key uses:
- **Hierarchical Temporal Indexing**: Shorter prefixes represent broader time intervals (e.g. multi-year or multi-day spans), while longer strings pinpoint narrow windows (minutes or seconds).
- **Compound Keys for Spatiotemporal Analytics**: Pairing `geohash + timehash` (e.g., `dqcjq1_ae0f0ba1fc`) enables fast multi-dimensional clustering, filtering, and key-value partitioning in databases (DuckDB, Bigtable, HBase, Redis, DynamoDB).
- **Sliding Window Queries**: Functions like `before()`, `after()`, `neighbors()`, and `expand()` allow boundary-aware window aggregations without expensive range scans.

---

## 2. Algorithm Specification & Invariants

### Epoch & Bounds
- Reference range: January 1, 1970 to January 1, 2098 (128-year total window).
- Resolution: Each additional character divides the time window by an octal factor of 8 (3 bits per character).

### Alphabet
- Characters: `'01abcdef'` (length 8)
- Neighbor mapping:
  - Sequence: `'0' -> '1' -> 'a' -> 'b' -> 'c' -> 'd' -> 'e' -> 'f'`
  - Preceding character loop: `'f01abcde'`
  - Succeeding character loop: `'1abcdef0'`

### Precision Hierarchy

| Precision (Length) | Approximate Window Size (± Window) |
|---|---|
| 0 | ± 128 years |
| 1 | ± 16 years |
| 2 | ± 2 years (~730 days) |
| 3 | ± 91.25 days (~3 months) |
| 4 | ± 11.406 days |
| 5 | ± 1.425 days (~34.2 hours) |
| 6 | ± 4.277 hours |
| 7 | ± 32.08 minutes |
| 8 | ± 4.01 minutes (~240.8 seconds) |
| 9 | ± 30.1 seconds |
| 10 | ± 3.76 seconds |

### Core Methods
- `encode(timestamp, precision=10)`: Converts Unix timestamp (seconds) into a timehash string.
- `decode(timehash_str)`: Returns `(time_window_start, time_window_end)`.
- `decode_exactly(timehash_str)`: Returns `(center_time, error_margin)`.
- `before(timehash_str)`: Returns the timehash of the preceding contiguous window at the same precision.
- `after(timehash_str)`: Returns the timehash of the succeeding contiguous window at the same precision.
- `neighbors(timehash_str)`: Returns `[before(hash), after(hash)]`.
- `expand(timehash_str)`: Returns `[before(hash), hash, after(hash)]`.

---

## 3. Repository Layout & Implementations

```
timehash/
├── AGENTS.md                 # Agent architectural guide (this file)
├── README.md                 # Markdown overview, quickstart, and reference
├── README.rst                # Original reStructuredText documentation
├── CHANGES.md                # Release version history
├── CONTRIBUTORS.md           # Author and contributor list
├── LICENSE                   # Modified BSD 3-Clause License
├── setup.py / setup.cfg      # Python package distribution metadata
├── MANIFEST.in               # Manifest for sdist packaging
├── test_sample.py            # Pytest test suite for Python implementation
│
├── Reference Implementations:
│   ├── timehash/             # Python package
│   │   └── __init__.py       # Canonical Python implementation
│   ├── timehash.go           # Golang implementation
│   ├── TimeHash.java         # Java implementation
│   ├── TimeHash.cs           # C# (.NET) implementation
│   ├── time_hash.h           # C header file implementation
│   ├── c-impl/               # C test and build files
│   │   ├── main.c
│   │   └── Makefile
│   └── timehash.pl           # Perl implementation
│
└── timehash-shaded-partial.png # Architectural diagram of sliding time windows
```

---

## 4. Development & Testing

### Python
```bash
python3 -m pytest test_sample.py
# Or with standard python
python3 test_sample.py
```

### Go
```bash
go test ./...  # or build timehash.go
```

### C Implementation
```bash
cd c-impl
make
./test
```

---

## 5. Guidelines for AI Agents

- **Cross-Language Consistency**: Any algorithm change must strictly preserve mathematical equivalence across Python, Go, Java, C, C#, and Perl implementations.
- **Bit-Level Invariance**: Do not change the alphabet sequence `01abcdef` or the base epoch calculations (1970–2098 window). Doing so invalidates historical indexes in external databases.
- **Deterministic Traversal**: `encode` and `decode` must remain deterministic and idempotent across platforms and timezones (all calculations operate in UTC Unix seconds).
