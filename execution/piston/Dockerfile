FROM ghcr.io/engineer-man/piston:latest

USER root

COPY dotnet/10.0.401 /piston/packages/dotnet/10.0.401

RUN cd /piston/packages/dotnet/10.0.401 \
    && bash build.sh \
    && env -i PWD=/piston/packages/dotnet/10.0.401 PATH=/usr/bin:/bin bash -c '. ./environment; env' \
        | grep -vE '^(PWD|OLDPWD|_|SHLVL)=' > .env \
    && touch .ppman-installed \
    && chown -R piston:piston /piston/packages/dotnet/10.0.401
