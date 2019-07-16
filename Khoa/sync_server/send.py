import io
from PIL import Image
import socket
import json
import time

message = {
    "filename": "log.html",
    "status": "PROCESSING"
}

json_str = json.dumps(message)

f = open(message["filename"], 'rb')
# byte_array = bytearray(f.read())

HOST, PORT = "10.38.32.171", 1000

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))

# Send file name
sock.sendall(bytes(json_str, 'utf8'))
time.sleep(2)

# Send file bytes
d = bytearray(f.read(1024))
while d:
    sock.sendall(d)
    res = sock.recv(1024)
    print(res)
    if ("OK" in str(res, 'utf8')):
        d = f.read(1024)
    else:
        print("error when sending")
        break

message["status"] = "DONE"
message["filename"] = ""
json_str = json.dumps(message)
time.sleep(2)
sock.sendall(bytes(json_str, 'utf8'))

f.close()
