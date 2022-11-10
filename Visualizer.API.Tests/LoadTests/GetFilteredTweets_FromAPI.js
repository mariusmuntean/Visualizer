import http from "k6/http";
import { check } from "k6";
import { Counter } from "k6/metrics";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";

export const requests = new Counter("http_reqs");

export const options = {
  stages: [
    { target: 60, duration: "5s" },
    { target: 30, duration: "1s" },
    { target: 10, duration: "1s" },
    { target: 0, duration: "2s" },
  ],
};

const query = `
query getFilteredTweets($filter: FindTweetsInputTypeQl!) {
    tweet {
        find(filter: $filter) {
            total
            tweets {
                id
                authorId
                username
                conversationId
                lang
                source
                text
                createdAt
                geoLoc {
                    latitude
                    longitude
                }
                entities {
                    hashtags
                    mentions
                }
                publicMetricsLikeCount
                publicMetricsRetweetCount
                publicMetricsReplyCount
            }
        }
    }
}
`;
const variables = { filter: { pageSize: 10, pageNumber: 1 } };
let headers = { "Content-Type": "application/json" };

export default function () {
  const res = http.post(
    "https://localhost:7083/graphql",
    JSON.stringify({ query: query, variables: variables }),
    { headers: headers }
  );

  const checkRes = check(res, {
    "status is 200": (r) => r.status === 200,
  });
}

// This will export to HTML as filename "result.html" AND also stdout using the text summary
// Making use of the handleSummary callback to export to HTML with K6-REPORTER https://github.com/benc-uk/k6-reporter
export function handleSummary(data) {
  return {
    "getfilteredtweets-api-result.html": htmlReport(data),
    stdout: textSummary(data, { indent: " ", enableColors: true }),
  };
}
