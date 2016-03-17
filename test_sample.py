from timehash import encode, before, after, neighbors, expand
from time import time


rightnow = time()
# (precision, window size) where window size is in seconds
precisions = [(8, 240.765380859375), (9, 30), (10, 3.76194)]
hashes = [encode(rightnow, precision) for (precision, _) in precisions]


def test_before():
    hashes_pre = [encode(rightnow - window_size, precision)
                  for (precision, window_size) in precisions]
    hashes_before = [before(hashcode) for hashcode in hashes]
    assert hashes_pre == hashes_before


def test_after():
    hashes_post = [encode(rightnow + window_size, precision)
                   for (precision, window_size) in precisions]
    hashes_after = [after(hashcode) for hashcode in hashes]
    assert hashes_post == hashes_after


def test_expand():
    hashes_real = [[encode(rightnow - window_size, precision),
                   encode(rightnow, precision),
                   encode(rightnow + window_size, precision)]
                   for (precision, window_size) in precisions]
    hashes_expanded = [expand(hashcode) for hashcode in hashes]
    assert hashes_real == hashes_expanded


def test_neighbors():
    hashes_real = [[encode(rightnow - window_size, precision),
                   encode(rightnow + window_size, precision)]
                   for (precision, window_size) in precisions]
    hashes_neighbors = [neighbors(hashcode) for hashcode in hashes]
    assert hashes_real == hashes_neighbors
