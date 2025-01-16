import os

path = r"C:/Users/macke/Downloads/google-10000-english.txt"
dest = r"C:/Users/macke/Downloads/4_letters.txt"

with open(path, 'r') as fpIn, open(dest, 'w') as fpOut:
    lineNumber = 0
    lineBreak = 0
    for line in fpIn:
        lineNumber += 1
        lineBreak += 1
        line = line.rstrip()
        if (len(line) == 4 and lineNumber % 2 == 0):
            if (lineBreak % 10 == 0):
                fpOut.write(("\"{}\",\n").format(line))
            else:
                fpOut.write(("\"{}\", ").format(line))

