#!/bin/sh
set -e

echo "Waiting for Redis to be available..."

until redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" ping | grep PONG; do
  sleep 1
done

echo "Seeding Redis database with test businesses..."

redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" GEOADD biz:geo -122.31665938469888 47.59996796458999 1
redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" GEOADD biz:geo -122.64987949582144 45.55924712430121 2

redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" SET 1 "Saigon Vietnam Deli"
redis-cli -h redis -p 6379 -a "$REDIS_PASSWORD" SET 2 "Mémoire Cà Phê"

echo "Done seeding Redis!"
