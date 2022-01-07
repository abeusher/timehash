Release process
===============

1. Install release requirements: `pip install wheel twine`
2. Build the project: `python setup.py sdist bdist_wheel`
3. Verify the packages: `twine check dist/*`
4. Test the upload to PyPI: `twine upload --repository-url https://test.pypi.org/legacy/ dist/*`
5. Test install: `pip install -i https://test.pypi.org/simple/ timehash`
6. Upload to PyPI: `twine upload dist/*`
