import cv2
import numpy as np
import cv2.aruco

def resize(img, scale_percent = 60):
    '''
    Resize function
    '''
    width = int(img.shape[1] * scale_percent / 100)
    height = int(img.shape[0] * scale_percent / 100)
    dim = (width, height)
    img = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
    return img

def getHomographyMatrix(corners, target):
    # print(type(corners))
    # print(corners)
    #Transformation de coordonnees image [pixel] vers coordonnees damier [cm]
    m, mask = cv2.findHomography(corners, target, cv2.RANSAC, 5.0)
    np.savetxt("matrix.txt", m, delimiter=',')
    return m

def extract(corners):
    new_corners = np.zeros(shape=(len(corners)*4,2))
    for cnt,corner in enumerate(corners):
        for x, c in enumerate(corner[0]):
            new_corners[cnt*4+x] = c
    return new_corners

image = cv2.imread('C:/Users/diogo.lopesdas/Desktop/aruco_detection/test_images/testaruco20.jpg')
image2 = cv2.imread('C:/Users/diogo.lopesdas/Desktop/aruco_detection/test_images/testaruco20.jpg')

# resize image
image = resize(image, 15)
image2 = resize(image2, 15)

cv2.imshow('projected arucos', image)
cv2.imshow('physical arucos', image2)
cv2.waitKey(0)
cv2.destroyAllWindows()

# (B, G, R) = cv2.split(image)
# Threshold not needed, cv2.aruco.detectMarkers function already does a threshold
# ret, B = cv2.threshold(B,90,255, cv2.THRESH_BINARY)
# ret, R = cv2.threshold(R,90,255, cv2.THRESH_BINARY)
# cv2.waitKey(0)
# cv2.destroyAllWindows()

arucoDict = cv2.aruco.getPredefinedDictionary(cv2.aruco.DICT_4X4_100)
arucoParams = cv2.aruco.DetectorParameters_create()

(corners1, ids1, rejected1) = cv2.aruco.detectMarkers(image, arucoDict, parameters=arucoParams)
cv2.aruco.drawDetectedMarkers(image, corners1)

(corners2, ids2, rejected2) = cv2.aruco.detectMarkers(image2, arucoDict, parameters=arucoParams)
cv2.aruco.drawDetectedMarkers(image2, corners2)

cv2.imshow('image3', image)
cv2.imshow('image4', image2)
cv2.waitKey(0)
cv2.destroyAllWindows()

corners1 = extract(np.asarray(corners1))
corners2 = extract(np.asarray(corners2))

matrix = getHomographyMatrix(corners1, corners2)
result = cv2.warpPerspective(image,matrix,(len(image[0]),len(image[1])))

cv2.imshow('result', result)
cv2.waitKey(0)
cv2.destroyAllWindows()