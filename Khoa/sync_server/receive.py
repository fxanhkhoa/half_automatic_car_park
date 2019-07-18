import io
from PIL import Image
import socket
import json
import time
import os

message = {
    "filename": "plate0638718.jpg",
    "status": "PROCESSING",
    "mode": "DOWNLOAD"
}


json_str = json.dumps(message)

if os.path.exists(message["filename"]):
    os.remove(message["filename"])
    print('removed')

f = open(message["filename"], 'ab')
# byte_array = bytearray(f.read())

HOST, PORT = "10.38.32.171", 1000

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))

# Send file name
sock.sendall(bytes(json_str, 'utf8'))
signal = True
while (signal):
    res = sock.recv(2048)
    try:
        file_name_length = int(res[-1])
        filename = str(res[-(file_name_length + 1):-1], 'utf8')
        res = res[:-(file_name_length + 1)]
        print(res)
    except Exception as e:
        print(e)
    try:
        data = str(res, 'utf8')
        if ("DONE" in data):
            signal = False
    except Exception as e:
        print('error here :', e)
        f.write(res)
        sock.sendall(bytes("OK", 'utf8'))

f.close()
