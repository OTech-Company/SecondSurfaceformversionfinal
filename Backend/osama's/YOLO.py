import cv2
from ultralytics import YOLO
import socket
import json
import threading

# Load YOLO model
model = YOLO('yolov8_custom//best1.pt')  # Replace with the path to your trained model

# Define target objects and their custom confidence thresholds
target_objects = {
    'tomato': 0.42,        # Threshold: 30%
    'bell_pepper': 0.4,   # Threshold: 40%
    'onion': 0.55          # Threshold: 70%
}

def start_server():
    # Set up the socket server
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('172.20.10.2', 9001))  # Change to an unused port
    server_socket.listen(1)
    print("Server is listening on port 8080...")

    client_socket, client_address = server_socket.accept()
    print(f"Client connected: {client_address}")

    # Open the video source (camera) after connection
    cap = cv2.VideoCapture(1, cv2.CAP_DSHOW)  # Use DirectShow backend
    if not cap.isOpened():
        print("Failed to open the camera. Exiting.")
        client_socket.close()
        server_socket.close()
        return

    # Set camera properties
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
    cap.set(cv2.CAP_PROP_FPS, 30)

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                print("Failed to read video frame.")
                break

            # Resize frame for YOLO detection
            resized_frame = cv2.resize(frame, (640, 640))

            # Run YOLO detection
            results = model(resized_frame)

            # Initialize an array to store detected objects for this frame
            detected_objects = []

            # Process YOLO results
            for result in results:
                if result.boxes:
                    for box, cls, prob in zip(result.boxes.xyxy, result.boxes.cls, result.boxes.conf):
                        class_name = result.names[int(cls)]
                        if class_name in target_objects:
                            confidence_threshold = target_objects[class_name]
                            if prob >= confidence_threshold:
                                detected_objects.append(class_name)

                                # Draw bounding boxes on the frame
                                x1, y1, x2, y2 = map(int, box)
                                cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 0), 2)
                                label = f"{class_name} {prob:.2f}"
                                cv2.putText(frame, label, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2)

                                # Add AR overlay (circle for object position)
                                cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
                                cv2.circle(frame, (cx, cy), 10, (0, 0, 255), -1)
                                cv2.putText(frame, f"AR: {class_name}", (cx + 15, cy), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 0), 2)

            # Convert detected objects to a space-separated string
            detected_objects_str = " ".join(detected_objects)

            # Send detected objects to the client
            client_socket.sendall(detected_objects_str.encode('utf-8'))

            # Break loop on 'q' key press
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    except Exception as e:
        print(f"Error: {e}")
    finally:
        cap.release()
        client_socket.close()
        server_socket.close()

if _name_ == "_main_":
    server_thread = threading.Thread(target=start_server)
    server_thread.start()