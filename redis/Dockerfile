FROM redis:7.2-alpine

COPY scripts/seed-redis.sh /seed-redis.sh

RUN chmod +x /seed-redis.sh

ENTRYPOINT ["/bin/sh", "/seed-redis.sh"]
