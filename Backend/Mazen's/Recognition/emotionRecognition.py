import cv2
import threading
from deepface import DeepFace

class EmotionRecognitionThread(threading.Thread):
    def __init__(self, frameQueue, outputQueue):
        threading.Thread.__init__(self)
        self.running = True
        self.frameQueue = frameQueue
        self.outputQueue = outputQueue
        self.faceCascade = cv2.CascadeClassifier(cv2.data.haarcascades + 'haarcascade_frontalface_default.xml')

    def run(self):
        while self.running:
            if not self.frameQueue.empty():
                frame = self.frameQueue.get()
                try:
                    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
                    faces = self.faceCascade.detectMultiScale(gray, scaleFactor=1.1, minNeighbors=5, minSize=(30, 30))
                    
                    if len(faces) > 0:
                        frameRGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                        result = DeepFace.analyze(frameRGB, actions=["emotion"], enforce_detection=True)
                        
                        if isinstance(result, list):
                            for faceResult in result:
                                emotion = faceResult.get("dominant_emotion", None)
                                if emotion:
                                    self.outputQueue.put(emotion)
                        elif isinstance(result, dict):
                            emotion = result.get("dominant_emotion", None)
                            if emotion:
                                self.outputQueue.put(emotion)
                    else:
                        self.outputQueue.put("No face detected in the frame.")
                except Exception as e:
                    self.outputQueue.put(f"Error: {str(e)}")

    def stop(self):
        self.running = False
