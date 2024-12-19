import cv2
import time
import pickle
import mediapipe as mp
from dollarpy import Recognizer, Template, Point

mp_drawing = mp.solutions.drawing_utils  # Drawing helpers
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic  # Mediapipe Solutions

templates = []  # list of templates for $1 training


def getRightHandPoint(videoURL, label):
    cap = cv2.VideoCapture(videoURL)
    with mp_holistic.Holistic(
        min_detection_confidence=0.5, min_tracking_confidence=0.5
    ) as holistic:
        # Initialize lists to store points with unique stroke IDs
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

        while cap.isOpened():
            ret, frame = cap.read()

            if ret:
                target_width = 480
                height, width = frame.shape[:2]
                aspect_ratio = height / width
                new_height = int(target_width * aspect_ratio)
                frame = cv2.resize(frame, (target_width, new_height))

                image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                image.flags.writeable = False
                results = holistic.process(image)

                image.flags.writeable = True
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
                mp_drawing.draw_landmarks(
                    image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS
                )

                try:
                    if results.right_hand_landmarks:
                        right_wrist.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.WRIST
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.WRIST
                                ].y,
                                1,
                            )
                        )
                        right_thumb_cmc.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_CMC
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_CMC
                                ].y,
                                2,
                            )
                        )
                        right_thumb_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_MCP
                                ].y,
                                3,
                            )
                        )
                        right_thumb_ip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_IP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_IP
                                ].y,
                                4,
                            )
                        )
                        right_thumb_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.THUMB_TIP
                                ].y,
                                5,
                            )
                        )
                        right_index_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_MCP
                                ].y,
                                6,
                            )
                        )
                        right_index_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_PIP
                                ].y,
                                7,
                            )
                        )
                        right_index_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_DIP
                                ].y,
                                8,
                            )
                        )
                        right_index_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.INDEX_FINGER_TIP
                                ].y,
                                9,
                            )
                        )
                        right_middle_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_MCP
                                ].y,
                                10,
                            )
                        )
                        right_middle_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_PIP
                                ].y,
                                11,
                            )
                        )
                        right_middle_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_DIP
                                ].y,
                                12,
                            )
                        )
                        right_middle_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.MIDDLE_FINGER_TIP
                                ].y,
                                13,
                            )
                        )
                        right_ring_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_MCP
                                ].y,
                                14,
                            )
                        )
                        right_ring_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_PIP
                                ].y,
                                15,
                            )
                        )
                        right_ring_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_DIP
                                ].y,
                                16,
                            )
                        )
                        right_ring_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.RING_FINGER_TIP
                                ].y,
                                17,
                            )
                        )
                        right_pinky_mcp.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_MCP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_MCP
                                ].y,
                                18,
                            )
                        )
                        right_pinky_pip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_PIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_PIP
                                ].y,
                                19,
                            )
                        )
                        right_pinky_dip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_DIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_DIP
                                ].y,
                                20,
                            )
                        )
                        right_pinky_tip.append(
                            Point(
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_TIP
                                ].x,
                                results.right_hand_landmarks.landmark[
                                    mp_holistic.HandLandmark.PINKY_TIP
                                ].y,
                                21,
                            )
                        )
                except:
                    pass

                cv2.imshow(label, image)
            else:
                cap.release()
                cv2.destroyAllWindows()
                cv2.waitKey(100)
                break

            if cv2.waitKey(10) & 0xFF == ord("q"):
                cap.release()
                cv2.destroyAllWindows()
                cv2.waitKey(100)
                break

    cap.release()
    cv2.destroyAllWindows()
    # Combine all points into one list
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
    print(f"All the Points with Length: {len(points)} are Saved with Label: {label}")
    return points


points = getRightHandPoint("Code/MediaPipe Dataset/Add to Cart 1.mp4", "Add to Cart 1")
print(points[0])


points = getRightHandPoint("Code/MediaPipe Dataset/Add to Cart 1.mp4", "Add to Cart 1")
templates.append(Template("Add to Cart 1", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Add to Cart 2.mp4", "Add to Cart 2")
templates.append(Template("Add to Cart 2", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Add to Cart 3.mp4", "Add to Cart 3")
templates.append(Template("Add to Cart 3", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Add to Cart 4.mp4", "Add to Cart 4")
templates.append(Template("Add to Cart 4", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Checkout 1.mp4", "Checkout 1")
templates.append(Template("Checkout 1", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Checkout 2.mp4", "Checkout 2")
templates.append(Template("Checkout 2", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Checkout 3.mp4", "Checkout 3")
templates.append(Template("Checkout 3", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Checkout 4.mp4", "Checkout 4")
templates.append(Template("Checkout 4", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Home 1.mp4", "Home 1")
templates.append(Template("Home 1", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Home 2.mp4", "Home 2")
templates.append(Template("Home 2", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Home 3.mp4", "Home 3")
templates.append(Template("Home 3", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Home 4.mp4", "Home 4")
templates.append(Template("Home 4", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Swipe Left 1.mp4", "Swipe Right 1")
templates.append(Template("Swipe Right 1", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Swipe Left 2.mp4", "Swipe Right 2")
templates.append(Template("Swipe Right 2", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Swipe Left 3.mp4", "Swipe Right 3")
templates.append(Template("Swipe Right 3", points))

points = getRightHandPoint("Code/MediaPipe Dataset/Swipe Left 4.mp4", "Swipe Right 4")
templates.append(Template("Swipe Right 4", points))


testPoints = getRightHandPoint("Code/MediaPipe Dataset/Add to Cart 5.mp4", "Add to Cart 5")
# testPoints = getRightHandPoint("Code/MediaPipe Dataset/Home 5.mp4", "Home 5")
# testPoints = getRightHandPoint("Code/MediaPipe Dataset/Swipe Left 5.mp4", "Swipe Right 5")
# testPoints = getRightHandPoint("Code/MediaPipe Dataset/Checkout 5.mp4", "Checkout 5")


start = time.time()
recognizer = Recognizer(templates)
result = recognizer.recognize(testPoints)
end = time.time()
print(f"The Predection is: {result[0]}")
print("time taken to classify:" + str(end - start))

print(result)

savePath = r"Models\Recognizer Model.pkl"

with open(savePath, "wb") as f:
    pickle.dump(recognizer, f)
