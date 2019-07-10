import xml.etree.ElementTree as ET
import cv2
import glob

# print(glob.glob("*.xml"))

all_file = glob.glob("*.xml")
# print(all_file)


scale_factor = 500 / 472

for each_file in all_file:
    imgpath = each_file.replace('xml','jpg')
    img = cv2.imread(imgpath)
    tree = ET.parse(each_file)
    root = tree.getroot()
    size = root.find('size')

    for child in root.findall("object"):
        # print(child.find('name').text)
        if child.find('name').text == 'plate':
            print(child)
            bndbox = child.find('bndbox')
            print(bndbox)
            xmin = int(bndbox.find('xmin').text) * scale_factor
            xmax = int(bndbox.find('xmax').text) * scale_factor
            ymin = int(bndbox.find('ymin').text) * scale_factor
            ymax = int(bndbox.find('ymax').text) * scale_factor
            # print(img.shape[0], img.shape[1])
            img = cv2.resize(img,(int(img.shape[1] * scale_factor),int(img.shape[0] * scale_factor)))
            # cv2.rectangle(img,(int(xmin), int(ymin)),(int(xmax), int(ymax)),(0,255,0),3)
            # cv2.imshow('drawed', img)
            print(img.shape[1], img.shape[0])

            ###### Write Image ######
            cv2.imwrite(imgpath, img)

            ###### Write to element ########
            bndbox.find('xmin').text = str(int(xmin))
            bndbox.find('ymin').text = str(int(ymin))
            bndbox.find('xmax').text = str(int(xmax))
            bndbox.find('ymax').text = str(int(ymax))

            size.find('width').text = str(img.shape[1])
            size.find('height').text = str(img.shape[0])
        elif child.find('name').text != 'plate':
            print(child)
            root.remove(child)

    tree.write(each_file)
    cv2.waitKey(0)
