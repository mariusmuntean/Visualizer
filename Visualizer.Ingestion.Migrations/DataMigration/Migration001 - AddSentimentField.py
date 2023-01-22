import json
import random

gb = GearsBuilder()

# Map each element to a tuple of key and JSON doc.
gb.map(lambda x: (x['key'], json.loads(execute('JSON.GET', x['key'], '$'))))

# Add or update the Sentiment field with a computed value, possibly based on other data.
gb.map(lambda tup: execute('JSON.SET', tup[0], '$.Sentiment', "\"" + random.choice(["Negative", "Neutral", "Positive"]) + "\""))

gb.run('TweetModel:*')
