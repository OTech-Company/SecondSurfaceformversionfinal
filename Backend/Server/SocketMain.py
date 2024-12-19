import os
import sys
import pickle
import socket
from threading import Thread
from ultralytics import YOLO

# Adjust path to include root directory
root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)

# Import necessary modules
from handleClient import handleClient
from sendToClient import sendDataToClient
from configFile import addConnectionInfo, loadJSON
from cameraFunctions import (
    CameraHandler,
    EmotionDetectionHandler,
    HandRecognitionHandler,
    YoloDetectionHandler,
)


def loadRecognizer(modelPath):
    """Loads the hand movement recognizer model from a pickle file."""
    with open(modelPath, "rb") as f:
        return pickle.load(f)


def startServer():
    """Starts the server and initializes handlers dynamically based on macAddress."""
    # Initialize variables
    macAddress = None
    emotionDetectionHandler = None
    handMovementHandler = None
    yoloDetectionHandler = None

    # Set up the server
    addConnectionInfo("Server/config.json")
    server = loadJSON("Server/config.json").get("server")
    port = server.get("port")
    serverIP = server.get("IP")

    cameraHandler = CameraHandler()
    recognizer = loadRecognizer("Models/Recognizer Model.pkl")
    yoloModel = YOLO("Models/yolo.pt")

    try:
        socketServer = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        socketServer.bind((serverIP, port))
        socketServer.listen(10)
        print(
            f"Server is running on {serverIP} : {port} and waiting for connections..."
        )

        while True:
            clientSocket, addr = socketServer.accept()
            print(f"Accepted connection from {addr}")

            # Pass `macAddress` as a mutable list to handleClient
            macAddressWrapper = [macAddress]

            # Handle client operations
            handleClient(
                clientSocket,
                cameraHandler,
                lambda value: macAddressWrapper.__setitem__(0, value),
            )

            # If macAddress is set, initialize handlers
            if macAddressWrapper[0] and not emotionDetectionHandler:
                macAddress = macAddressWrapper[0]
                print(f"MAC Address saved: {macAddress}")

                # Initialize handlers
                emotionDetectionHandler = EmotionDetectionHandler(cameraHandler)
                handMovementHandler = HandRecognitionHandler(cameraHandler, recognizer)
                yoloDetectionHandler = YoloDetectionHandler(cameraHandler, yoloModel)

                # Handle Messages to the Client
                Thread(
                    target=sendDataToClient,
                    args=(
                        clientSocket,
                        handMovementHandler.outputQueue,
                        yoloDetectionHandler.outputQueue,
                    ),
                    daemon=True,
                ).start()

    finally:
        # Cleanup resources
        cameraHandler.stop()
        if emotionDetectionHandler:
            emotionDetectionHandler.stop()
        if handMovementHandler:
            handMovementHandler.stop()
        if yoloDetectionHandler:
            yoloDetectionHandler.stop()
        if socketServer:
            socketServer.close()


# Start the server
startServer()
