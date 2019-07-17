from utils import upload

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

class ThreadedTCPRequestHandler(socketserver.BaseRequestHandler):

    def handle(self):
        print("{} connected".format(self.client_address))
        clients.append(self.request)
        close = 0
        self.filename = ''
        self.isfilename = False
        self.done = False
        while not close:
            try:
                buf = self.request.recv(2048)  # max 52428800
                try:
                    data = str(buf, 'utf8')
                    print(data)
                    json_obj = json.loads(data)
                    self.filename = json_obj["filename"]
                    
                    if (".jpg" in self.filename):
                        self.folder_file = "images/" + self.filename
                    elif (".html" in self.filename):
                        self.folder_file = "logs/" + self.filename

                    if os.path.exists(self.folder_file) and self.filename != '':
                        os.remove(self.folder_file)
                    
                    self.isfilename = True
                    if (json_obj["status"] == "PROCESSING"):
                        self.done = False
                    else:
                        self.done = True

                    print("Is File Name \n")
                except Exception as e:
                    # print(e)
                    self.isfilename = False
                if not buf:
                    print('Disconnected: ', self.client_address)
                    clients.remove(self.request)
                    close = 1
                    return
                # response = bytes("{}: {}".format(cur_thread.name, data), 'utf8')
                # self.request.sendall(response)
                print(self.filename, self.isfilename, self.done)
                if (self.isfilename == False) and (self.done == False):
                    print("writing")
                    #### Write file ####
                    length = int(buf[-1])
                    print(length)
                    new_buf = buf[:-(length + 1)]
                    print(len(new_buf))
                    f = open(self.folder_file, 'ab')
                    f.write(new_buf)
                    f.close()
                    print("write OK")
                    self.request.sendall(bytes("OK", 'utf8'))
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