#!/bin/bash
set -e

for project in ./src/RestExceptions ./src/RestExceptions.FluentValidation; do
  (
    cd "$project"
    dotnet clean
    dotnet build -c Release
    dotnet pack -c Release
  )
done

# To run:
# chmod +x build-pack.sh
