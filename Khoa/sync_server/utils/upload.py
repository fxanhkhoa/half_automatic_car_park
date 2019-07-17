import socket
import threading

class upload (threading.Thread):
    def __init__(self, request_client, location, data):
        threading.Thread.__init__(self)
        self.status = False
        self.request_client = request_client
        self.data = data
        self.location = location

    def run(self):
        try:
            self.get_file_name()
            f = open(self.filename, 'ab')
            f.write(self.data)
            f.close()
        except Exception as e:
            print(e)
            pass
    
    def set_status(self, status):
        self.status = status

    def send_signal_received(self):
        self.request_client.sendall(bytes("OK", 'utf8'))

    def get_file_name(self):
        file_name_length = int(self.data[-1])
        self.filename = self.data[-file_name_length:-1]
        self.data = self.data[:-(file_name_length + 1)]

