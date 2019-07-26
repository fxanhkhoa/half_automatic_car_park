from utils.upload import upload
from utils.download import download

import socket
import socketserver
import json

import signal
import sys

import _thread
import threading

import os.path
import os


clients = [] #### For list of clients

def isJson(jsonStr):
    try:
        json.loads(jsonStr)
        print("is Json")
        return True
    except Exception as e:
        print("Not Json")
        return False

class ThreadedTCPRequestHandler(socketserver.BaseRequestHandler):

    def handle(self):
        print("{} connected".format(self.client_address))
        clients.append(self.request)
        close = 0
        self.isfilename = False
        self.mode = ''
        self.filename = ''
        while not close:
            try:
                buf = self.request.recv(2048)  # max 52428800
                try:
                    data = ''
                    data = str(buf, 'utf8')
                    # print(data)
                except Exception as e:
                    print("Error when change str", e)

                ## Check if json ##
                if isJson(data):
                    json_object = json.loads(data)
                    self.mode = json_object['mode']
                    self.filename = json_object['filename']
                    self.download_process = download(self.request, self.filename)
                    if (self.mode == 'UPLOAD'):
                        ## Remove 1st time ##
                        file_location = upload.get_location(self.filename)
                        if not os.path.exists(file_location):
                            os.makedirs(file_location)
                        if os.path.exists(self.filename):
                            os.remove(self.filename)
                    
                    elif (self.mode == "DOWNLOAD"):
                        self.download_process = download(self.request, self.filename)
                        self.download_process.start()
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

    ###### Signal part
    signal.signal(signal.SIGINT, signal_handler)
    print('Press Ctrl+C to stop')
    # signal.pause()

    
    ip, port = server.server_address

    # Start a thread with the server -- that thread will then start one
    # more thread for each request
    
    # Exit the server thread when the main thread terminates
    server_thread.daemon = True
    server_thread.start()

    print("Server loop running in thread:", server_thread.name)
    server.serve_forever()


    # server.shutdown()
    # server.server_close()