# StockTrader
The repo contains a UWP application that uses machine learning to know when to buy and sell stocks.

To execute the Tensorflow script tf_test.py, ensure that the file NSE_TATAGLOBAL.csv is stored in the same directory.
I'll upload the dependencies so that it can be executed in a virtual environment, but for now,  all the expected imports are at the top of the script.
Then, "python tf_test.py" using python 3 in the directory should be enough to execute.
To check logs, first create a "logs" folder in the same directory as the script, then
open another cmd terminal and type in "tensorboard --logdir = logs/" 
