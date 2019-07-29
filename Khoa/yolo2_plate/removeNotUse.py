import os
import glob
from shutil import copyfile

all_file_xml = glob.glob("GreenParking_Ann/*.xml")
all_file_images = glob.glob("GreenParking/*.jpg")

for img in all_file_images:
    try:
        xmlName = img.replace(".jpg", ".xml")
        xmlNameDst = xmlName.replace("GreenParking", "GreenParking_New")
        xmlNameSrc = xmlName.replace("GreenParking", "GreenParking_Ann")
        copyfile(xmlNameSrc, xmlNameDst)
    except Exception as e:
        print(e)

print("done")