#!/bin/bash
set -eu

PROJECT=Hangman
STARTUP_PROJECT=Hangman
SQL_CONTEXT_CLASS=Hangman.Infrastructure.SqlContext

function apply_migrations() {
    echo "Applying migrations..."
    dotnet ef database update --verbose \
        --project ${PROJECT} \
        --startup-project ${STARTUP_PROJECT} \
        --context ${SQL_CONTEXT_CLASS}
}

apply_migrations
