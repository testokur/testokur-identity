#!/bin/bash
docker pull nazmialtun/testokur-identity:latest
docker stop testokur-identity && docker rm testokur-identity --force
docker run --cap-add=SYS_PTRACE --security-opt seccomp=unconfined  -d \
	--env-file /home/env/identity.env \
	-v /home/cert:/app/cert
	--name testokur-identity \
	--restart=unless-stopped \
	--network=testokur \
	--network-alias=identity \
	nazmialtun/testokur-identity:latest