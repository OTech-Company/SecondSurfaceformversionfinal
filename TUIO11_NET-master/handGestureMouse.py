import cv2
import mediapipe as mp
import pyautogui
import time
import math

# Initialize Mediapipe hands and drawing utilities
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
hands = mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7)

# Set up video capture and screen size
cap = cv2.VideoCapture(0)
screen_width, screen_height = pyautogui.size()

# Variables to track position and time
start_time = time.time()  # Initialize start time to current time
last_position = None
hold_time_threshold = 1.5  # seconds
position_tolerance = 15  # pixels, adjust for more/less tolerance

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break
    
    # Flip the frame horizontally for a mirror view
    frame = cv2.flip(frame, 1)
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb_frame)

    # Check if hand landmarks are detected
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            # Get the index finger tip coordinates
            x_index = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP].x
            y_index = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP].y

            # Convert normalized coordinates to screen coordinates
            screen_x = int(screen_width * x_index)
            screen_y = int(screen_height * y_index)

            # Move the mouse to the screen coordinates
            pyautogui.moveTo(screen_x, screen_y)

            # Check if the finger has stayed in the same approximate position
            current_position = (screen_x, screen_y)
            if last_position is None:
                last_position = current_position
                start_time = time.time()  # Reset start time when initializing position
            else:
                # Calculate distance from last position
                distance = math.sqrt((current_position[0] - last_position[0]) ** 2 +
                                     (current_position[1] - last_position[1]) ** 2)
                
                # Check if within tolerance
                if distance <= position_tolerance:
                    # Calculate elapsed time only if start_time is set
                    elapsed_time = time.time() - start_time if start_time else 0
                    if elapsed_time >= hold_time_threshold:
                        pyautogui.click()  # Perform left-click
                        start_time = time.time()  # Reset start time after clicking
                else:
                    # Update position and reset the timer if moved outside tolerance
                    last_position = current_position
                    start_time = time.time()

            # Draw landmarks on the frame
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    # Display the frame
    cv2.imshow('Hand Gesture Control', frame)

    # Break loop on 'q' key press
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release resources
cap.release()
cv2.destroyAllWindows()
