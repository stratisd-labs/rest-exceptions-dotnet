#!/bin/bash
set -e

# Packages are copied to <repo>/releases, regardless of where the script is run from
releases="$(cd "$(dirname "$0")" && pwd)/releases"

for project in ./src/RestExceptions ./src/RestExceptions.FluentValidation; do
  (
    cd "$(dirname "$0")/$project"
    dotnet clean
    dotnet build -c Release
    dotnet pack -c Release -o "$releases"
  )
done

# To run:
# chmod +x build-pack.sh
