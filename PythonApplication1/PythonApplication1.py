#this is meant to be a test of passing data to and from the C# end of the application.
import sys

def print_val(x):
    print("The value passed to Python is {}!".format(x))

def main():
    print("Hello from Python!")
    print_val(sys.argv[1])

main()