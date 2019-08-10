import threading
import os
import json
import time

class watcher (threading.Thread):
   def __init__(self, location):
      threading.Thread.__init__(self)
      self.location = location
      self.run_flag = True

   def run(self):
      try:
         while self.run_flag:
            try:
               print("====== WATCHER FOLDER ====== ", self.location)
               for r, d, f in os.walk(self.location):
                  for efile in f:
                     print("===== WATCHER FILE: =====", r , " ", d,efile)
                     if ("FileTable.json" == efile):
                        continue
                     if (not self.exist(r + "\\" + efile)):
                        print("===== DELETING FILE: =====", efile)
                        os.remove(r + "/" + efile)
            except Exception as ex:
               print("====== WATCHER ERROR INSIDE: ", ex, " ======")

            time.sleep(10)
      except Exception as e:
         print("====== WATCHER ERROR: ", e, " ======")

   def stop_all(self):
      self.run_flag = False

   def exist(self, file):
      print("====== WATCHER CHECKING EXIST ====== ", file)
      with open(self.location + "/FileTable.json", 'r') as f:
         distros_dict = json.load(f)
      for element in distros_dict:
         print("====== WATCHER ELEMENT ====== ", element["filename"])
         if (file == element["filename"]):
            print("===== WATCHER EXIST =====")
            return True
         else:
            print("===== WATCHER CONTINUE CHECKING =====")
      return False