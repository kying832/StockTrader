import flask
import tensorflow as tf
import numpy as np
import pandas as pd
from tensorflow import keras
from sklearn import preprocessing
from tensorflow.python.keras.backend import set_session

app = flask.Flask(__name__)
model = None
sess = None
#https://kobkrit.com/tensor-something-is-not-an-element-of-this-graph-error-in-keras-on-flask-web-server-4173a8fe15e1
#this function will load the pre-trained model into the app at the start.
def load_model_into_app():
    global model
    model =  tf.keras.models.load_model("90 - SEQ - 5 - FUTURE LENGTH - 1559688600.HDF5")



@app.route("/", methods = ['GET'])
def home():
    return "Hello World!"

@app.route("/test", methods = ['POST'])
def post_test():
    data = {"success": False}
    inc_data = flask.request.get_json()
    data.update(inc_data)
#    if data["value"] > 50:
#        data["bignum"] = "Yes"
    data["success"] = True
    print(data)
    return flask.jsonify(data)

#this route will be the primary API interaction with the UWP app.
#the app will send time series data via POST request of say 6 months worth of closing stock prices
#the function will then use the stock prices to predict the buy/sell signal.
#code adapted from the Keras documentation found here: https://blog.keras.io/building-a-simple-keras-deep-learning-rest-api.html
@app.route("/predict", methods = ['POST'])
def ML_predict():
    global sess
    global graph
    with graph.as_default():
        set_session(sess)
        sess.run(initializer)
        data = {"success": False}
        try:
        # inc_data = flask.request.get_json()
            #inc_data will contain an excess of data like {date : ..., opening : ..., closing :..., etc}
            #process inc_data into something that keras' model can tolerate
            #the keras model has been trained on 90 day sequences.  so, need to pare down the api call from 6 months to 90 days
            #raw_data = pd.DataFrame.from_dict([inc_data])
            raw_data = pd.DataFrame(flask.request.get_json())
            print(raw_data.head())
            #hack out unnecessary columns
            seq = raw_data[["date", "close", "volume"]].copy()
            #pare down to last 90 rows for the model
            seq = seq.iloc[-91:]
            #and scale to set up the model
            seq["close"] = seq["close"].pct_change()
            seq["volume"] = seq["volume"].pct_change()
            seq.dropna(inplace=True)
            seq["close"] = preprocessing.scale(seq["close"].values)
            seq["volume"] = preprocessing.scale(seq["volume"].values)

            seq.set_index("date",inplace=True)
            #TOFIX - Scaling leaves some values outside of range [-1, 1].  This may trip up the model?
            #should also check how the model is scaling training data.  
            seq.dropna(inplace=True)
            #process data from dataframe into numpy array?
            vals = np.array(seq.values)
            print(vals.shape)
            vals = np.reshape(vals, (1, 90, 2))
            print(vals.shape)
            #print(seq.head())
            #print(type(seq["close"][1]))
            
            #now use the model to make a prediction based on the sequence
            prediction = model.predict(vals)

            #parse the prediction into something useful
            data["prediction"] = int(np.argmax(prediction))

            data["success"] = True
        except:
            print("exception of some kind")

        finally:
            return flask.jsonify(data)


if __name__ == "__main__":
    load_model_into_app()
    initializer = tf.global_variables_initializer()
    graph = tf.get_default_graph()
    sess = tf.Session()
    app.run(debug=True)