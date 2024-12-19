import json
import time


def sendDataToClient(clientSocket, mpDataQueue, yoloDataQueue):
    """
    This function will be continuously checking if new data is available in the dataQueue and sending it to the client.
    It works in parallel with the handleClient function.
    """
    with clientSocket:
        while True:
            if not mpDataQueue.empty():
                data = mpDataQueue.get()
                if data:
                    try:
                        message = {"operation": "MediaPipe", "data": data}
                        print(f"Sending data: {message}")
                        clientSocket.sendall(json.dumps(message).encode("utf-8"))
                    except Exception as e:
                        print(f"Error sending data: {e}")
            if not yoloDataQueue.empty():
                data = yoloDataQueue.get()
                if data:
                    try:
                        message = {"operation": "YOLO", "data": data}
                        print(f"Sending data: {message}")
                        clientSocket.sendall(json.dumps(message).encode("utf-8"))
                    except Exception as e:
                        print(f"Error sending data: {e}")
            time.sleep(1)  # Sleep for 1 second before checking again
