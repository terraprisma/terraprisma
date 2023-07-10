#!/bin/bash

# Originally used aliases, env vars are better and more consistent.

if [[ -f "dotnet/dotnet" ]]; then
    echo "using local dotnet"
    # alias dotnet="dotnet/dotnet"
    dotnet="dotnet/dotnet"
elif [[ -f "dotnet/dotnet.exe" ]]; then
    echo "using local dotnet"
    # alias dotnet="dotnet/dotnet.exe"
    dotnet="dotnet/dotnet.exe"
else
    echo "using global dotnet"
    # alias dotnet="dotnet"
    dotnet="dotnet"
fi

$dotnet "$@"
