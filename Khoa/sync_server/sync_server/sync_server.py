from utils.upload import upload
from utils.download import download
from watcher.watcher import watcher

import socket
import socketserver
import json
from filehash import FileHash

import signal
import sys

import _thread
import threading
import queue

import os.path
import os
import sys
import colorama
from termcolor import colored, cprint


clients = [] #### For list of clients

def isJson(jsonStr):
    try:
        json.loads(jsonStr)
        print("is Json")
        return True
    except Exception as e:
        print("Not Json")
        return False

def GetMD5(filename):
    if not os.path.exists(filename):
        return "NONE"
    md5_hasher = FileHash('md5')
    md5_str = md5_hasher.hash_file(filename)
    return md5_str

def CheckSumMD5(filename, md5hash):
    if not os.path.exists(filename):
        return False
    md5_hasher = FileHash('md5')
    md5_str = md5_hasher.hash_file(filename)
    md5_str = md5_str.upper()
    print("Comparing :", md5_str, md5hash)
    if (md5_str == md5hash):
        return True
    else:
        return False

class ThreadedTCPRequestHandler(socketserver.BaseRequestHandler):

    def handle(self):
        print("{} connected".format(self.client_address))
        clients.append(self.request)
        close = 0
        self.isfilename = False
        self.mode = ''
        self.filename = ''
        self.download_process = download(self.request, self.filename)

        while not close:
            try:
                buf = self.request.recv(2048)  # max 52428800
                # print(buf)
                try:
                    data = ''
                    data = str(buf, 'utf8')
                    # print(data)
                except Exception as e:
                    print("Error when change str", e)

                ## Check if json ##
                if isJson(data):
                    json_object = json.loads(data)
                    print(data)
                    self.filename = ""
                    if (json_object["status"] == "OK"):
                        print("set flag")
                        self.download_process.set_flag(True)
                    else:
                        self.mode = json_object['mode']
                        self.filename = json_object['filename']
                        print(self.filename)
                        if (self.mode == 'UPLOAD'):
                            ## Remove 1st time ##
                            file_location = upload.get_location(self.filename)
                            if file_location != '':
                                if not os.path.exists(file_location):
                                    os.makedirs(file_location)
                            if os.path.exists(self.filename):
                                os.remove(self.filename)

                        elif (self.mode == "DOWNLOAD"):
                            print("create")
                            self.download_process.request_client = self.request
                            self.download_process.filename = self.filename
                            self.download_process.start()

                        elif (self.mode == "CHECKMD5"):
                            print("Checking MD5 \n")
                            message = {
                                "filename": self.filename,
                                "status": "",
                                "mode": "CHECKMD5"
                            }
                            if (CheckSumMD5(self.filename, json_object["status"])):
                                message["status"] = "SAME"
                            else:
                                message["status"] = "DIFF"
                            json_str = json.dumps(message)
                            print("JSON TO SEND: ", json_str)
                            self.request.sendall(bytes(json_str, 'utf8'))
                        
                        elif (self.mode == "LOCATION"):
                            self.watcher = watcher(self.filename)
                            self.watcher.start()
                                
                ## Got OK When Download mode ##
                elif "OK" == data:
                    self.download_process.set_flag(True)
                elif "FAIL" == data:
                    self.download_process.set_flag(True)
                    self.download_process.set_stop(True)
                else: # Not Json
                    self.upload_process = upload(self.request, buf)
                    self.upload_process.start()
                    
                ##### Handle disconnect #####
                if not buf:
                    print('Disconnected: ', self.client_address)
                    clients.remove(self.request)
                    close = 1
                    return

            ##### Handle disconnect #####        
            except Exception as e:
                print(e)
                print('Disconnected: ', self.client_address)
                clients.remove(self.request)
                self.watcher.stop_all()
                close = 1

class ThreadedTCPServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    pass

s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.connect(("8.8.8.8", 80))
print(s.getsockname()[0])
# Port 0 means to select an arbitrary unused port
HOST, PORT = str(s.getsockname()[0]), 1000
s.close()
server = ThreadedTCPServer((HOST, PORT), ThreadedTCPRequestHandler)
server_thread = threading.Thread(target=server.serve_forever)

def signal_handler(sig, frame):
    def kill_me_please(server):
        server.shutdown()
    _thread.start_new_thread(kill_me_please, (server,))
    print('You pressed Ctrl+C!')
    sys.exit(0)

if __name__ == "__main__":
    
    colorama.init()

    ###### Signal part
    signal.signal(signal.SIGINT, signal_handler)
    cprint('Press Ctrl+C to stop', 'red', 'on_cyan')
    # signal.pause()

    # for r, d, f in os.walk("My_Work\\Python\\half_automatic_car_park\\Khoa\\sync_application\\sync_application\\bin\\Debug\\test_folder"):
    #     for efile in f:
    #        print("===== WATCHER FILE: =====", efile) 

    
    ip, port = server.server_address

    # Start a thread with the server -- that thread will then start one
    # more thread for each request
    
    # Exit the server thread when the main thread terminates
    server_thread.daemon = True
    server_thread.start()

    cprint("Server loop running in thread:" + server_thread.name, 'green')
    server.serve_forever()


    # server.shutdown()
    # server.server_close()
