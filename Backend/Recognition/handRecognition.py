import cv2
import json
import threading
import mediapipe as mp
from dollarpy import Point


class HandMovementRecognitionThread(threading.Thread):
    def __init__(self, frameQueue, outputQueue, recognizer):
        threading.Thread.__init__(self)
        self.running = True
        self.frameCount = 0
        self.collectedFrames = []
        self.recognizer = recognizer
        self.frameQueue = frameQueue
        self.outputQueue = outputQueue
        self.mpHolistic = mp.solutions.holistic
        self.mpDrawing = mp.solutions.drawing_utils
        self.holistic = self.mpHolistic.Holistic(
            min_detection_confidence=0.5, min_tracking_confidence=0.5
        )

    def run(self):
        while self.running:
            if not self.frameQueue.empty():
                frame = self.frameQueue.get()
                try:
                    # Process the frame and collect hand points
                    pointsArray = self.getRightHandPoints([frame])

                    if pointsArray:
                        # Right hand detected
                        self.frameCount += 1
                        self.collectedFrames.append(frame)

                    if self.frameCount % 5 == 0 and self.frameCount > 0:
                        print(f"Collected {len(self.collectedFrames)} frames")
                        if self.collectedFrames:
                            # Send collected frames to getRightHandPoints function for recognition
                            points = self.getRightHandPoints(self.collectedFrames)

                            try:
                                # Attempt to recognize the action using the recognizer
                                action = self.recognizer.recognize(points)
                                if action:
                                    if action[1] > 0.75:
                                        self.outputQueue.put(action[0])
                            except Exception as e:
                                # Handle any exceptions that occur during the process
                                print(
                                    f"An error occurred while recognizing the action: {e}"
                                )

                            # Reset the collected frames for the next batch
                            self.collectedFrames = []

                except Exception as e:
                    self.outputQueue.put(f"Error: {str(e)}")

    def getRightHandPoints(self, frames):
        # Initialize lists for each hand landmark (reset on every frame)
        (
            right_wrist,
            right_thumb_cmc,
            right_thumb_mcp,
            right_thumb_ip,
            right_thumb_tip,
        ) = ([], [], [], [], [])
        right_index_mcp, right_index_pip, right_index_dip, right_index_tip = (
            [],
            [],
            [],
            [],
        )
        right_middle_mcp, right_middle_pip, right_middle_dip, right_middle_tip = (
            [],
            [],
            [],
            [],
        )
        right_ring_mcp, right_ring_pip, right_ring_dip, right_ring_tip = [], [], [], []
        right_pinky_mcp, right_pinky_pip, right_pinky_dip, right_pinky_tip = (
            [],
            [],
            [],
            [],
        )
        for frame in frames:
            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            image.flags.writeable = False
            results = self.holistic.process(image)

            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            self.mpDrawing.draw_landmarks(
                image, results.right_hand_landmarks, self.mpHolistic.HAND_CONNECTIONS
            )

            try:
                if results.right_hand_landmarks:
                    # Safely collect points for right hand landmarks (with checks for each part)
                    if results.right_hand_landmarks:
                        right_wrist.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.WRIST
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.WRIST
                                ].y,
                                1,
                            )
                        )
                        right_thumb_cmc.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_CMC
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_CMC
                                ].y,
                                2,
                            )
                        )
                        right_thumb_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_MCP
                                ].y,
                                3,
                            )
                        )
                        right_thumb_ip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_IP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_IP
                                ].y,
                                4,
                            )
                        )
                        right_thumb_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.THUMB_TIP
                                ].y,
                                5,
                            )
                        )
                        right_index_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_MCP
                                ].y,
                                6,
                            )
                        )
                        right_index_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_PIP
                                ].y,
                                7,
                            )
                        )
                        right_index_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_DIP
                                ].y,
                                8,
                            )
                        )
                        right_index_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.INDEX_FINGER_TIP
                                ].y,
                                9,
                            )
                        )
                        right_middle_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_MCP
                                ].y,
                                10,
                            )
                        )
                        right_middle_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_PIP
                                ].y,
                                11,
                            )
                        )
                        right_middle_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_DIP
                                ].y,
                                12,
                            )
                        )
                        right_middle_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.MIDDLE_FINGER_TIP
                                ].y,
                                13,
                            )
                        )
                        right_ring_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_MCP
                                ].y,
                                14,
                            )
                        )
                        right_ring_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_PIP
                                ].y,
                                15,
                            )
                        )
                        right_ring_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_DIP
                                ].y,
                                16,
                            )
                        )
                        right_ring_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.RING_FINGER_TIP
                                ].y,
                                17,
                            )
                        )
                        right_pinky_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_MCP
                                ].y,
                                18,
                            )
                        )
                        right_pinky_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_PIP
                                ].y,
                                19,
                            )
                        )
                        right_pinky_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_DIP
                                ].y,
                                20,
                            )
                        )
                        right_pinky_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    self.mpHolistic.HandLandmark.PINKY_TIP
                                ].y,
                                21,
                            )
                        )

            except Exception as e:
                print(f"Error in processing hand landmarks: {e}")
                return []
        # Combine all collected points into one list
        points = (
            right_wrist
            + right_thumb_cmc
            + right_thumb_mcp
            + right_thumb_ip
            + right_thumb_tip
            + right_index_mcp
            + right_index_pip
            + right_index_dip
            + right_index_tip
            + right_middle_mcp
            + right_middle_pip
            + right_middle_dip
            + right_middle_tip
            + right_ring_mcp
            + right_ring_pip
            + right_ring_dip
            + right_ring_tip
            + right_pinky_mcp
            + right_pinky_pip
            + right_pinky_dip
            + right_pinky_tip
        )

        return points

    def stop(self):
        self.running = False
