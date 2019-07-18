import socket
import threading

class download (threading.Thread):
    @staticmethod
    def get_location(filename):
        if (".jpg" in filename):
            location = "images/" + filename
        elif (".html" in filename):
            location = "logs/" + filename
        return location

    def __init__(self, request_client, filename):
        threading.Thread.__init__(self)
        self.status = False
        self.request_client = request_client
        self.filename = filename
        self.flag_continue = False
        self.stop = False

    def run(self):
        try:
            location = download.get_location(self.filename)
            f = open(location, 'rb')
            d = f.read(1024)
            while d:
                d = d + bytes(self.filename, 'utf8')
                d = d + bytes([len(self.filename)])
                self.request_client.sendall(d)
                while (not self.flag_continue):
                    pass

                if (self.stop):
                    return

                d = f.read(1024)
                self.flag_continue = False
            f.close()
            last_d = bytes("DONE", 'utf8')
            last_d = last_d + bytes(self.filename, 'utf8')
            last_d = last_d + bytes([len(self.filename)])
            self.request_client.sendall(last_d)
        except Exception as e:
            print('error is: ',e)
            pass
    
    def set_flag(self, flag = False):
        self.flag_continue = flag

    def set_stop(self, stop_signal = False):
        self.stop = stop_signal