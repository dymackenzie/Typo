import os

path = r"C:/Users/macke/Downloads/wordlist.10000.txt"
dest = r"C:/Users/macke/Downloads/5_letters.txt"

with open(path, 'r') as fpIn, open(dest, 'w') as fpOut:
    lineNumber = 0
    for line in fpIn:
        lineNumber += 1
        line = line.rstrip()
        if (len(line) == 5 and lineNumber % 10 == 0):
            fpOut.write(("\"{}\", ").format(line))

