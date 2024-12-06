import cv2
import threading
import mediapipe as mp


class HandMovementRecognitionThread(threading.Thread):
    def __init__(self, frameQueue, outputQueue, recognizer):
        threading.Thread.__init__(self)
        self.running = True
        self.frameCount = 0
        self.collectedPoints = []
        self.recognizer = recognizer
        self.frameQueue = frameQueue
        self.outputQueue = outputQueue
        self.mp_holistic = mp.solutions.holistic
        self.mp_drawing = mp.solutions.drawing_utils
        self.holistic = self.mp_holistic.Holistic(
            min_detection_confidence=0.5, min_tracking_confidence=0.5
        )

    def run(self):
        while self.running:
            if not self.frameQueue.empty():
                frame = self.frameQueue.get()
                try:
                    # Process the frame and collect hand landmarks
                    landmarks = self.getRightHandPoints(frame)

                    if landmarks:
                        # Right hand detected
                        self.frameCount += 1
                        self.collectedPoints.append(landmarks)
                    else:
                        # No right hand detected, reset everything
                        self.frameCount = 0
                        self.collectedPoints = []

                    # Every 50 frames, process the collected points and send them for recognition
                    if self.frameCount % 50 == 0 and self.frameCount > 0:
                        if self.collectedPoints:
                            # Combine all collected points into one list (flattening it)
                            combinedPoints = [
                                (
                                    point,
                                    strokeIDx,
                                )  # Each point paired with its corresponding stroke ID
                                for strokeIDx, sublist in enumerate(
                                    self.collectedPoints, 1
                                )  # Start strokeIDx at 1
                                for point in sublist
                            ]
                            self.outputQueue.put(combinedPoints)

                            # Recognize the action based on the combined points
                            action = self.recognizer.recognize_action(combinedPoints)
                            if action:
                                self.outputQueue.put(action)

                            # Reset the collected points for the next batch
                            self.collectedPoints = []

                    self.frameCount += 1

                except Exception as e:
                    self.outputQueue.put(f"Error: {str(e)}")
