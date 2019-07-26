import os

path = 'location1/location2/a.txt'

location = os.path.dirname(path)

if not os.path.exists(location):
   os.makedirs(location)
   
f = open("location/abc.txt", 'wb')
f.write(bytes('asdasd' ,'utf8'))
f.close()