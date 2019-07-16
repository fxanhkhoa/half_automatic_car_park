import io
from PIL import Image
import socket
import json
import time

message = {
    "filename": "log.html"
}

json_str = json.dumps(message)

f = open(message["filename"], 'rb')
byte_array = bytearray(f.read())

HOST, PORT = "10.38.32.171", 1000

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))

# Send file name
sock.sendall(bytes(json_str, 'utf8'))
time.sleep(2)
# Send file bytes
sock.sendall(byte_array)

f.close()
