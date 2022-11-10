import redis from "k6/experimental/redis";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { Trend } from "k6/metrics";

const RedisLatencyMetric = new Trend("redis_latency", true);

export const options = {
  stages: [
    { target: 60, duration: "5s" },
    { target: 30, duration: "1s" },
    { target: 10, duration: "1s" },
    { target: 0, duration: "2s" },
  ],
};
// More details here https://k6.io/docs/javascript-api/k6-experimental/redis/client/
const redisClient = new redis.Client({
  addrs: new Array("localhost:6379"),
  password: "",
});

export default function () {
  const start = Date.now();

  redisClient.sendCommand(
    "FT.SEARCH",
    "tweetmodel-idx",
    "(@HasGeoLoc:{1})",
    "LIMIT",
    "0",
    "10",
    "SORTBY",
    "Username",
    "ASC"
  );

  const latency = Date.now() - start;
  RedisLatencyMetric.add(latency);
}

export function handleSummary(data) {
  return {
    "getfilteredtweets-redis-result.html": htmlReport(data),
    stdout: textSummary(data, { indent: " ", enableColors: true }),
  };
}
