import cv2
import mediapipe as mp
import pyautogui
import math
import time

# Initialize Mediapipe hands and drawing utilities
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
hands = mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7)

# Set up video capture and screen size
cap = cv2.VideoCapture(0)
screen_width, screen_height = pyautogui.size()

# Variable to track if click has been triggered, the last open-hand position, and the last click time
click_triggered = False
last_open_x, last_open_y = None, None
last_click_time = 0  # Track the time of the last click

# Define the upward offset and freeze duration (0.5 seconds)
upward_offset = 10
freeze_duration = 0.5  # 0.5 seconds

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
            # Get the index finger tip and thumb tip coordinates
            x_index = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP].x
            y_index = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP].y
            x_thumb = hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].x
            y_thumb = hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].y

            # Convert normalized coordinates to screen coordinates
            screen_x = int(screen_width * x_index)
            screen_y = int(screen_height * y_index)

            # Calculate the distance between index finger and thumb
            distance = math.sqrt((x_index - x_thumb) ** 2 + (y_index - y_thumb) ** 2)

            # Check if the hand is "closed" (fingers are close together)
            if distance < 0.05:
                current_time = time.time()
                # Only click if 0.5 seconds have passed since the last click
                if not click_triggered and (current_time - last_click_time) > freeze_duration:
                    pyautogui.click()  # Perform left-click
                    click_triggered = True  # Prevent repeated clicking while hand is closed
                    last_click_time = current_time  # Update last click time

                # Move the mouse slightly up when the hand is closed
                if last_open_x is not None and last_open_y is not None:
                    pyautogui.moveTo(last_open_x, last_open_y - upward_offset)
            else:
                # Hand is open: Update the last open position and move the mouse
                click_triggered = False
                last_open_x, last_open_y = screen_x, screen_y
                pyautogui.moveTo(last_open_x, last_open_y)  # Move the mouse only when hand is open

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
