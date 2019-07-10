import os

### Create file ###
f = open("train.txt", "w")
f.write('')
f.close()

currentdir = os.getcwd()

arr = os.listdir(currentdir)
for elem in arr:
   if "jpg" in elem:
      elem = elem.replace('.jpg', '\n')
      f = open("train.txt", 'a')
      f.write(elem)
      f.close()

print('Generate done!')
