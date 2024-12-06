import os
import sys
import queue
import pickle
import socket
from threading import Thread

root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)
from handleClient import handleClient
from configFile import addConnectionInfo, loadJSON
from cameraFunctions import CameraHandler, EmotionDetectionHandler


modelPath = "Models/Recognizer Model.pkl"
with open(modelPath, "rb") as f:
    newRecognizer = pickle.load(f)


def startServer():
    cameraHandler = CameraHandler()
    addConnectionInfo("Server/config.json")
    config = loadJSON("Server/config.json")
    emotionDetectionHandler = EmotionDetectionHandler(cameraHandler)

    port = config.get("port")
    serverIP = config.get("IP")
    try:
        socketServer = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        socketServer.bind((serverIP, port))
        socketServer.listen(10)
        print(f"Server is running on {serverIP}:{port} and waiting for connections...")

        while True:
            clientSocket, addr = socketServer.accept()
            print(f"Accepted connection from {addr}")
            Thread(
                target=handleClient,
                args=(clientSocket, cameraHandler, emotionDetectionHandler),
                daemon=True,
            ).start()
    finally:
        cameraHandler.stop()
        emotionDetectionHandler.stop()
        if socketServer:
            socketServer.close()


startServer()
