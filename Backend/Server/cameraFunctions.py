import os
import sys
import cv2
import queue
import threading

root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)

from Recognition.yoloRecognition import YoloDetectionThread
from Recognition.emotionRecognition import EmotionRecognitionThread
from Recognition.handRecognition import HandMovementRecognitionThread
from Recognition.faceRecognition import FaceRecognitionThread, loadSavedUsers


def confirmLogin(bluetooth, userMacAddress):
    """Confirms login by checking the user's MAC address against detected devices."""
    for device in bluetooth:
        if device["MAC Address"] == userMacAddress:
            return True
    return False


class EmotionDetectionHandler:
    def __init__(self, cameraHandler):
        self.frameCount = 0
        self.isRunning = True
        self.outputQueue = queue.Queue()
        self.cameraHandler = cameraHandler
        self.frameQueue = queue.Queue(maxsize=5)
        self.processorThread = threading.Thread(target=self.processEmotions)
        self.emotionThread = EmotionRecognitionThread(self.frameQueue, self.outputQueue)
        self.emotionThread.start()
        self.processorThread.start()

    def processEmotions(self):
        """Processes frames for emotion detection in the background."""
        while self.isRunning:
            frame = self.cameraHandler.getFrame()
            if frame is None:
                continue

            self.frameCount += 1
            if self.frameCount % 200 == 0 and not self.frameQueue.full():
                self.frameQueue.put(frame)

    def stop(self):
        """Stops emotion detection and related threads."""
        self.isRunning = False
        self.emotionThread.stop()
        self.emotionThread.join()
        self.processorThread.join()


class YoloDetectionHandler:
    def __init__(self, cameraHandler, model):
        """
        Initializes the YOLO Detection Handler.
        Args:
            cameraHandler: An instance of CameraHandler to capture frames.
            model: A preloaded YOLO model for object detection.
        """
        self.isRunning = True
        self.outputQueue = queue.Queue()
        self.cameraHandler = cameraHandler
        self.frameQueue = queue.Queue(maxsize=5)
        self.processorThread = threading.Thread(target=self.processFrames)
        self.yoloThread = YoloDetectionThread(self.frameQueue, self.outputQueue, model)
        self.yoloThread.start()
        self.processorThread.start()

    def processFrames(self):
        """Continuously captures frames from the camera and queues them for YOLO detection."""
        while self.isRunning:
            frame = self.cameraHandler.getFrame()
            if frame is None:
                continue

            # Queue the frame if the frame queue is not full
            if not self.frameQueue.full():
                self.frameQueue.put(frame)

    def getDetections(self):
        """
        Retrieves detection results from the output queue.
        Returns:
            A list of detections or an empty list if no detections are available.
        """
        detections = []
        while not self.outputQueue.empty():
            detections.append(self.outputQueue.get())
        return detections

    def stop(self):
        """Stops the YOLO detection handler and related threads."""
        self.isRunning = False
        self.yoloThread.stop()
        self.yoloThread.join()
        self.processorThread.join()


class HandRecognitionHandler:
    def __init__(self, cameraHandler, recognizer):
        self.isRunning = True
        self.outputQueue = queue.Queue()
        self.cameraHandler = cameraHandler
        self.frameQueue = queue.Queue(maxsize=5)
        self.processorThread = threading.Thread(target=self.processHands)
        self.handThread = HandMovementRecognitionThread(
            self.frameQueue, self.outputQueue, recognizer
        )
        self.handThread.start()
        self.processorThread.start()

    def processHands(self):
        """Processes frames for emotion detection in the background."""
        while self.isRunning:
            frame = self.cameraHandler.getFrame()
            if frame is None:
                continue

            self.frameQueue.put(frame)

    def stop(self):
        """Stops emotion detection and related threads."""
        self.isRunning = False
        self.handThread.stop()
        self.handThread.join()
        self.processorThread.join()


class CameraHandler:
    def __init__(self):
        self.frame = None
        self.isRunning = True
        self.videoCapture = cv2.VideoCapture(1)
        self.captureThread = threading.Thread(target=self.captureFrames)
        self.captureThread.start()

    def captureFrames(self):
        """Continuously captures frames in a separate thread."""
        try:
            while self.isRunning:
                ret, frame = self.videoCapture.read()
                if not ret:
                    break
                self.frame = frame
                cv2.imshow("Video", frame)
                if cv2.waitKey(1) & 0xFF == ord("q"):
                    break
        finally:
            self.videoCapture.release()
            cv2.destroyAllWindows()

    def getFrame(self):
        """Returns the most recent frame captured by the camera."""
        return self.frame

    def stop(self):
        """Stops the camera and the capture thread."""
        self.isRunning = False
        self.captureThread.join()


def recognitionAndLogin(cameraHandler, allData, bluetooth):
    """Handles recognition and login functionality using a shared CameraHandler."""
    frameQueue = queue.Queue(maxsize=5)
    outputQueue = queue.Queue()

    recognitionThread, macAddress = startFaceRecognition(
        allData, frameQueue, outputQueue
    )

    try:
        while True:
            frame = cameraHandler.getFrame()
            if frame is None:
                continue

            if not frameQueue.full():
                frameQueue.put(frame)

            if not outputQueue.empty():
                name, userID = outputQueue.get()
                if name != "Unknown":
                    isFound = confirmLogin(bluetooth, macAddress[userID])
                    if isFound:
                        print(f"Recognized: {name}")
                        return {"User Name": name, "MAC Address": macAddress[userID]}

            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

    finally:
        recognitionThread.stop()
        recognitionThread.join()


def emotionDetection(cameraHandler):
    """Handles emotion detection functionality using a shared CameraHandler."""
    frameQueue = queue.Queue(maxsize=5)
    outputQueue = queue.Queue()

    emotionThread = startEmotionRecognition(frameQueue, outputQueue)
    frameCount = 0

    try:
        while True:
            frame = cameraHandler.getFrame()
            if frame is None:
                continue

            frameCount += 1
            if frameCount % 200 == 0 and not frameQueue.full():
                frameQueue.put(frame)

            if not outputQueue.empty():
                emotion = outputQueue.get()
                print(f"Detected Emotion: {emotion}")

            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

    finally:
        emotionThread.stop()
        emotionThread.join()


def startFaceRecognition(allData, frameQueue, outputQueue):
    """Initializes and starts the face recognition thread."""
    path = [data["faceImage"] for data in allData]
    faceNames = [data["name"] for data in allData]
    macAddress = [data["macAddress"] for data in allData]

    faceEncodings = loadSavedUsers(path)
    recognitionThread = FaceRecognitionThread(
        frameQueue, outputQueue, faceEncodings, faceNames
    )
    recognitionThread.start()

    return recognitionThread, macAddress


def startEmotionRecognition(frameQueue, outputQueue):
    """Initializes and starts the emotion recognition thread."""
    emotionThread = EmotionRecognitionThread(frameQueue, outputQueue)
    emotionThread.start()
    return emotionThread
