import socket
import threading
import os
import json
from watcher.watcher import watcher

import colorama
from termcolor import colored, cprint

class upload (threading.Thread):
    
    @staticmethod
    def get_location(filename):
        location = os.path.dirname(filename)
        cprint(location, 'blue')
        return location

    def __init__(self, request_client, data):
        threading.Thread.__init__(self)
        self.status = False
        self.request_client = request_client
        self.data = data


    def run(self):
        try:
            watcher.can_watch = False
            self.parse_location()
            # cprint("======= Removing Null byte ======", 'blue')
            # self.data = self.data.rstrip(b'\x00')
            cprint("======= Writing File ======", 'blue')
            f = open(self.filename, 'ab')
            f.write(self.data)
            cprint("======= Sending OK Signal ======", 'blue')
            self.send_signal_received()
            f.close()
            cprint("======= Sent OK Signal ======", 'blue')
        except Exception as e:
            cprint('error is: {}'.format(e), 'red')
            pass
    
    def set_status(self, status):
        self.status = status

    def send_signal_received(self):
        message = {
            "filename": self.filename,
            "status": "OK",
            "mode": "UPLOAD"
        }
        json_str = json.dumps(message)
        self.request_client.sendall(bytes(json_str, 'utf8'))

    def parse_location(self):
        file_name_length = int(self.data[-1])
        # print('len = ', file_name_length)
        cprint('file name array = {}'.format(self.data[-(file_name_length + 1):-1]), 'yellow')
        self.filename = str(self.data[-(file_name_length + 1):-1], 'utf8')
        cprint('file name = {}'.format(self.filename), 'yellow')
        self.location = upload.get_location(self.filename)
        self.data = self.data[:-(file_name_length + 1)]
        # print(self.data)
