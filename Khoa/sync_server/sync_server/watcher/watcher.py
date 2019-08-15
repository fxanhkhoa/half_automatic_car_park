import threading
import os
import json
import time

import colorama
from termcolor import colored, cprint

class watcher (threading.Thread):

   can_watch = True

   def __init__(self, location):
      threading.Thread.__init__(self)
      self.location = location
      self.run_flag = True

   def run(self):
      try:
         while self.run_flag:
            try:
               if (watcher.can_watch == False):
                  continue
               cprint("====== WATCHER FOLDER: {} ======".format(self.location), 'blue')
               for r, d, f in os.walk(self.location):
                  for efile in f:
                     cprint("===== WATCHER FILE: {} {} {} =====".format(r, d, efile), 'blue')
                     if ("FileTable.json" == efile):
                        continue
                     if (not self.exist(r + "\\" + efile)):
                        cprint("===== DELETING FILE: {} =====".format(efile), 'blue')
                        os.remove(r + "/" + efile)
            except Exception as ex:
               cprint("====== WATCHER ERROR INSIDE: {} ======".format(ex), 'red')

            time.sleep(10)
      except Exception as e:
         print("====== WATCHER ERROR: {} ======".format(e), 'red')

   def stop_all(self):
      self.run_flag = False

   def exist(self, file):
      cprint("====== WATCHER CHECKING EXIST {} ====== ".format(file), 'yellow')
      with open(self.location + "/FileTable.json", 'r') as f:
         distros_dict = json.load(f)
      for element in distros_dict:
         cprint("====== WATCHER ELEMENT ====== ".format(element["filename"]), 'yellow')
         if (file == element["filename"]):
            cprint("===== WATCHER EXIST =====" , 'yellow')
            return True
         else:
            cprint("===== WATCHER CONTINUE CHECKING =====", 'yellow')
      return False