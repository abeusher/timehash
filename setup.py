from setuptools import setup
from pathlib import Path
setup(
  name = 'timehash',
  packages = ['timehash'], # this must be the same as the name above
  version = '1.2',
  description = 'Module to encode and decode timestamps to/from TimeHashes',
  long_description = (Path(__file__).parent / 'README.rst').read_text(),
  long_description_content_type = 'text/x-rst',
  author = 'Abe Usher',
  author_email = 'abe.usher@gmail.com',
  url = 'https://github.com/abeusher/timehash',
  download_url = 'https://github.com/abeusher/timehash/tarball/1.2',
  keywords = ['timehash', 'datetime', 'time'],
  classifiers = [],
)

