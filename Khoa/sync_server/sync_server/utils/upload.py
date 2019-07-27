import socket
import threading
import os
import json

class upload (threading.Thread):
    
    @staticmethod
    def get_location(filename):
        location = os.path.dirname(filename)
        return location

    def __init__(self, request_client, data):
        threading.Thread.__init__(self)
        self.status = False
        self.request_client = request_client
        self.data = data


    def run(self):
        try:
            self.parse_location()
            f = open(self.filename, 'ab')
            f.write(self.data)
            self.send_signal_received()
            f.close()
        except Exception as e:
            print('error is: ',e)
            pass
    
    def set_status(self, status):
        self.status = status

    def send_signal_received(self):
        message = {
            "filename": "",
            "status": "OK",
            "mode": "UPLOAD"
        }
        json_str = json.dumps(message)
        self.request_client.sendall(bytes(json_str, 'utf8'))

    def parse_location(self):
        file_name_length = int(self.data[-1])
        self.filename = str(self.data[-(file_name_length + 1):-1], 'utf8')
        self.location = upload.get_location(self.filename)
        self.data = self.data[:-(file_name_length + 1)]
