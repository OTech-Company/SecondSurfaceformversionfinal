import os
import sys
import json
import threading

root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)
from bluetoothScan import main
from configFile import parseJSON
from cameraFunctions import recognitionAndLogin
from Database.handleDatabaseOperations import handleDatabaseOperation


# Load all the Models here


def handleClient(clientSocket, cameraHandler, emotionDetectionHandler):
    with clientSocket:
        data = clientSocket.recv(4096).decode("utf-8")
        if data:
            print(f"Received data: {data}")
            dataJSON = parseJSON(data)
            operation = dataJSON.get("operation")

            if operation == "Start":
                allData = handleDatabaseOperation('{"operation": "getAllMacFace"}')
                allData = allData.get("data")

                bluetoothResult = {}
                bluetoothThread = threading.Thread(
                    target=lambda: bluetoothResult.update({"result": main()})
                )
                bluetoothThread.start()
                bluetoothThread.join()
                bluetoothDevices = bluetoothResult.get("result", {})
                userInfo = recognitionAndLogin(cameraHandler, allData, bluetoothDevices)
                if userInfo:
                    operation = {
                        "operation": "getRecommendation",
                        "macAddress": userInfo.get("MAC Address"),
                    }
                    allData = handleDatabaseOperation(json.dumps(operation))
                    allData = allData.get("recommendations")
                    response = {"operation": "Recommendations", "data": allData}
                    clientSocket.sendall(json.dumps(response).encode("utf-8"))

            elif operation == "Page":
                allEmotions = []
                while not emotionDetectionHandler.outputQueue.empty():
                    emotion = emotionDetectionHandler.outputQueue.get()
                    if emotion in [
                        "Angry",
                        "Disgust",
                        "Fear",
                        "Happy",
                        "Sad",
                        "Surprise",
                        "Neutral",
                    ]:
                        allEmotions.append(emotion)
                print(allEmotions)
                return
