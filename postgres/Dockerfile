FROM postgres:16

COPY scripts/*.sql /docker-entrypoint-initdb.d/

RUN chown postgres:postgres /docker-entrypoint-initdb.d/*.sql
RUN chmod 644 /docker-entrypoint-initdb.d/*.sql
