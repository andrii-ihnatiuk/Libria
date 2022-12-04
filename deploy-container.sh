#!/bin/bash

deploy_container() 
{
    CONTAINER_NAME=$1

    # docker-compose.yml directory
    cd /srv/docker

    # stop a running container by name
    docker compose stop ${CONTAINER_NAME}

    # pull the newest image from hub
    docker compose pull ${CONTAINER_NAME}

    # start a container without building an image
    docker compose up -d --no-deps --no-build ${CONTAINER_NAME}

    # remove all unused images without confirmation
    docker image prune --all --force
}

deploy_container $@