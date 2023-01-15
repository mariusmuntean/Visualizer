import json
import random

gb = GearsBuilder()

# Map each element to a tuple of key and JSON doc.
gb.map(lambda x: (x['key'], json.loads(execute('JSON.GET', x['key'], '$'))))

# Add or update the SentimentConfidence field with a computed value, possibly based on other data.
gb.map(lambda tup: execute('JSON.SET', tup[0], '$.SentimentConfidence', "\"" + str(random.uniform(0, 1)) + "\""))

gb.run('TweetModel:*')
