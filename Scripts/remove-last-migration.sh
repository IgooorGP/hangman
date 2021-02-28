#!/bin/bash
set -eu

PROJECT=Hangman
STARTUP_PROJECT=Hangman
SQL_CONTEXT_CLASS=Hangman.Infrastructure.SqlContext

function remove_last_migration() {
    FORCE_MIGRATION=$1

    if [ $FORCE_MIGRATION == true ]; then    
        echo "Removing last migration with force..."
        dotnet ef migrations remove --verbose \
            --project ${PROJECT} \
            --startup-project ${STARTUP_PROJECT} \
            --context ${SQL_CONTEXT_CLASS} \
            --force
    else
        echo "Removing last migration without forcing..."
        dotnet ef migrations remove --verbose \
            --project ${PROJECT} \
            --startup-project ${STARTUP_PROJECT} \
            --context ${SQL_CONTEXT_CLASS}
    fi
}

function parse_command_line_args() {    
    for ARGUMENT in "$@"
    do
        KEY=$(echo $ARGUMENT | cut -f1 -d=)
        VALUE=$(echo $ARGUMENT | cut -f2 -d=)  # to capture values

        case "$KEY" in
            --force)       FORCE=true ;;
            *)
        esac
    done

    FORCE=${FORCE:-false}
}

parse_command_line_args "$@"
remove_last_migration $FORCE