#testing
#import packages
#code adapted from https://pythonprogramming.net/crypto-rnn-model-deep-learning-python-tensorflow-keras/
import pandas as pd
import numpy as np
from collections import deque
#to plot within notebook
import matplotlib.pyplot as plt
import random
import time
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, LSTM, BatchNormalization
from tensorflow.keras.callbacks import TensorBoard
from tensorflow.keras.callbacks import ModelCheckpoint
from tensorflow import keras
#setting figure size
from matplotlib.pylab import rcParams
rcParams['figure.figsize'] = 20,10

#for normalizing data
from sklearn import preprocessing
SEQ_LEN = 90 #take 60 days worth of stock prices into consideration
FUTURE_PERIOD = 5 #and forecast 1 week ahead.

EPOCHS = 10
BATCH_SIZE = 64
NAME = f"{SEQ_LEN} - SEQ - {FUTURE_PERIOD} - FUTURE LENGTH - {int(time.time())}"

def classify(cur, fut):
	if float(fut) > float(cur):
		return 1 #buy/hold signal
	else:
		return 0 #sell/ignore signal

def process_df(df):
	df = df.drop("future", 1) #drop extra column
	for col in df.columns:	#normalize columns to percentage change
		if col != "target":
			df[col] = df[col].pct_change()
			df.dropna(inplace = True)
			df[col] = preprocessing.scale(df[col].values)
			#df[col] = preprocessing.maxabs_scale(df[col].values)
		df.dropna(inplace=True)
	#build list sequences containing 60 days of closing data
	seq_data = [] #this list contains the sequences.
	sequence = deque(maxlen = SEQ_LEN) #use a deque to sequentially add newer trading days and remove older extras
	for i in df.values:
		sequence.append([n for n in i[:-1]]) #append all data except for the target column
		if len(sequence) == SEQ_LEN:
			seq_data.append([np.array(sequence), i[-1]]) #once the deque is at desired length, begin adding sequences to the list
		
	random.shuffle(seq_data)	#randomize the sequences
	#split the data into buy/hold vs sell/ignore
	buys = []
	sells = []
	
	for seq, target in seq_data:
		if target == 0:
			sells.append([seq, target])
		else:
			buys.append([seq, target])
	#truncate the data to the smaller of the two to ensure the same size.
	
	smaller_list = min(len(buys), len(sells))
	if(smaller_list != 0):
		buys = buys[:smaller_list]
		sells = sells[:smaller_list]
	#recombine sequences and shuffle.
	seq_data = buys + sells
	random.shuffle(seq_data)
	#split features and results into 2 separate lists for TF to chew on
	X = []
	y = []
	#X contains the sequences
	#Y contains labels (buy/hold vs sell/ignore)
	for seq, target in seq_data:
		X.append(seq)
		y.append(target)
	return np.array(X), y
	



#read the file
df = pd.read_csv('MacroTrends_Data_Download_AAPL.csv')

#print the head
#print(df.head())

#setting index as date
df['Date'] = pd.to_datetime(df.Date,format='%Y-%m-%d')
df.index = df['Date']
df = df[['Close', 'Volume']]
#df = df[::-1]	#reverse the dataframe to have the oldest entries first
df['future'] = df['Close'].shift(-FUTURE_PERIOD)

df['target'] = list(map(classify, df['Close'], df['future']))

print(df.head(10))
#split data set into training and validation data.
#will split training data into 1st 95% of data and validate with the last 5%
times = sorted(df.index.values)
last_5_index = int(0.90 *  len(times))
print(last_5_index)
train_df = df[:last_5_index]
validation_df = df[last_5_index:]

print(train_df.tail())

train_x, train_y = process_df(train_df)
valid_x, valid_y = process_df(validation_df)

print(train_x[1])

print(f"train data: {len(train_x)} validation: {len(valid_x)}")
print(f"Sell/Ignore: {train_y.count(0)}  Buy/Hold: {train_y.count(1)}")
print(f"VALIDATION SET: Sell/Ignore: {valid_y.count(0)}  Buy/Hold: {valid_y.count(1)}")

#begin building the tensorflow model
model = Sequential()
model.add(LSTM(128, kernel_regularizer=keras.regularizers.l2(0.001), activation = 'softsign' ,input_shape = (train_x.shape[1:]), return_sequences = True))
model.add(Dropout(0.4))
model.add(BatchNormalization())

model.add(LSTM(128, kernel_regularizer=keras.regularizers.l2(0.001), activation = 'softsign',  return_sequences = True))
model.add(Dropout(0.4))
model.add(BatchNormalization())

model.add(LSTM(128, kernel_regularizer=keras.regularizers.l2(0.001), activation = 'softsign' ))
model.add(Dropout(0.4))
model.add(BatchNormalization())

model.add(Dense(32, activation =  'softsign'))
model.add(Dropout(0.4))

model.add(Dense(2, activation = 'softmax'))

#compile model
#opt = tf.keras.optimizers.dam(lr=1e-3, decay=1e-6)
opt = tf.keras.optimizers.Nadam()
model.compile(loss = 'sparse_categorical_crossentropy', optimizer = opt, metrics = ['accuracy'])
tensorboard = TensorBoard(log_dir="logs/{}".format(NAME))
#train model
history = model.fit(train_x, train_y, batch_size=BATCH_SIZE, epochs = EPOCHS, validation_data = (valid_x, valid_y), callbacks = [tensorboard])

#score model
score = model.evaluate(valid_x, valid_y, verbose = 0)
print('Test loss:', score[0])
print('Test accuracy', score[1])

#save model
model.save("models/{}".format(NAME))

#plot
#plt.figure(figsize=(16,8))		
#plt.plot(df['Close'], label='Close Price history')
#uncomment to show the plot of stock price data
#plt.show()


