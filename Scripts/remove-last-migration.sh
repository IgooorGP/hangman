#!/bin/bash
set -eu

PROJECT=Hangman
STARTUP_PROJECT=Hangman
SQL_CONTEXT_CLASS=Hangman.Infrastructure.SqlContext

function remove_last_migration {
    echo "Removing last migration..."
    dotnet ef migrations remove --verbose \
        --project ${PROJECT} \
        --startup-project ${STARTUP_PROJECT} \
        --context ${SQL_CONTEXT_CLASS}
}

remove_last_migration