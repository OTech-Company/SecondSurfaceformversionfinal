import cv2
import threading
import numpy as np
import face_recognition


class FaceRecognitionThread(threading.Thread):
    def __init__(self, frameQueue, outputQueue, faceEncodings, faceNames):
        threading.Thread.__init__(self)
        self.frameQueue = frameQueue
        self.outputQueue = outputQueue
        self.faceEncodings = faceEncodings
        self.faceNames = faceNames
        self.running = True

    def run(self):
        while self.running:
            if not self.frameQueue.empty():
                frame = self.frameQueue.get()
                name = self.recognizeFaces(frame)
                self.outputQueue.put(name)

    def stop(self):
        self.running = False

    def recognizeFaces(self, frame):
        # Resize the frame for faster processing
        smallFrame = cv2.resize(frame, (0, 0), fx=0.25, fy=0.25)
        rgbSmallFrame = cv2.cvtColor(smallFrame, cv2.COLOR_BGR2RGB)
        faceLocations = face_recognition.face_locations(rgbSmallFrame)
        newFaceEncodings = face_recognition.face_encodings(rgbSmallFrame, faceLocations)

        if newFaceEncodings:
            distances = []
            for newFaceEncoding in newFaceEncodings:
                faceDistances = face_recognition.face_distance(
                    self.faceEncodings, newFaceEncoding
                )
                distances.append(np.min(faceDistances))

            nearestFaceIndex = np.argmin(distances)
            nameOfUser = "Unknown"

            matches = face_recognition.compare_faces(
                self.faceEncodings, newFaceEncodings[nearestFaceIndex]
            )
            bestMatchIndex = np.argmin(
                face_recognition.face_distance(
                    self.faceEncodings, newFaceEncodings[nearestFaceIndex]
                )
            )
            if matches[bestMatchIndex] and distances[nearestFaceIndex] < 0.7:
                nameOfUser = self.faceNames[bestMatchIndex]

            return [nameOfUser, bestMatchIndex]
        return ["Unknown", -1]


# This Function will change once there is a Database
def loadSavedUsers(facePath):
    knownFaceEncodings = []
    for path in facePath:
        pImage = face_recognition.load_image_file(path)
        knownFaceEncodings.append(face_recognition.face_encodings(pImage)[0])
    return knownFaceEncodings
