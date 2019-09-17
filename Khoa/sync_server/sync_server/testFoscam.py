import cv2

cap = cv2.VideoCapture("rtsp://foscam:khoa03021996@14.169.1.30:554/videoMain")
print(cap.isOpened())

while True:
	try:
		ret, frame = cap.read()
		#print(r)
		cv2.imshow('aa', frame)
		
		if cv2.waitKey(1) & 0xFF == ord('q'):
			break
	except Exception as e:
		print(e)

cv2.destroyAllWindows()