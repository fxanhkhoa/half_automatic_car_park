import socket
import threading
import os
import json
import queue
from .Global import Global

import colorama
from termcolor import colored, cprint

class download (threading.Thread):

    @staticmethod
    def get_location(filename):
        location = os.path.dirname(filename)
        return location

    def __init__(self, request_client, filename):
        threading.Thread.__init__(self)
        self.status = False
        self.request_client = request_client
        self.filename = filename
        self.flag_continue = False
        self.stop = False
        self.lock = threading.Lock()
        # self.lock.acquire()

    def run(self):
        try:
            count = 0
            f = open(self.filename, 'rb')
            d = f.read(1024)
            while d:
                d = d + bytes(self.filename, 'utf8')
                d = d + bytes([len(self.filename)])
                self.request_client.sendall(d)
                print("getting acquire")
                # self.lock.acquire()
                while (self.flag_continue == False):
                    pass
            
                if (self.stop):
                    return

                d = f.read(1024)
                self.flag_continue = False
                count = count + 1
            f.close()

            # Send finish signal
            message = {
                "filename": self.filename,
                "status": "DONE",
                "mode": "DOWNLOAD"
            }
            json_str = json.dumps(message)
            self.request_client.sendall(bytes(json_str, 'utf8'))
            cprint("sent {} times".format(count), 'blue')
        except Exception as e:
            print('error is: ',e)
    
    def set_flag(self, flag = False):
        self.flag_continue = flag
        # self.lock.release()
        print(self.flag_continue, flag)

    def set_stop(self, stop_signal = False):
        self.stop = stop_signal