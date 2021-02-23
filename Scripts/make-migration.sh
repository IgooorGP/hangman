#!/bin/bash
set -eu

PROJECT=Hangman
STARTUP_PROJECT=Hangman
SQL_CONTEXT_CLASS=Hangman.Infrastructure.SqlContext

function make_migration {
    MIGRATION_NAME=$1

    echo "Creating migration $1..."
    dotnet ef migrations add $1 \
        --verbose \
        --project ${PROJECT} \
        --startup-project ${STARTUP_PROJECT} \
        --context ${SQL_CONTEXT_CLASS}
}

make_migration $1