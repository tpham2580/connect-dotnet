#!/bin/sh
set -e

echo "Waiting for Redis to be available..."

until redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" ping | grep PONG; do
  sleep 1
done

echo "Seeding Redis database with test businesses..."

redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" GEOADD biz:geo -100.335167 40.608013 101
redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" GEOADD biz:geo -50.330000 80.610000 202
redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" GEOADD biz:geo -122.340000 47.612000 303

echo "Done seeding Redis!"
