import os
import sys
import json
import threading

# Adjust path to include root directory
root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)

from bluetoothScan import main
from configFile import parseJSON
from cameraFunctions import recognitionAndLogin
from Database.handleDatabaseOperations import handleDatabaseOperation


def handleClient(clientSocket, cameraHandler, setMacAddress):
    """Handles client requests and updates macAddress."""
    with clientSocket:
        print("Client connection established.")
        data = clientSocket.recv(4096).decode("utf-8")
        if data:
            print(f"Received data: {data}")
            dataJSON = parseJSON(data)
            operation = dataJSON.get("operation")

            if operation == "Start":
                print("Operation 'Start' initiated.")
                # Process "Start" operation
                allData = handleDatabaseOperation('{"operation": "getAllMacFace"}').get(
                    "data"
                )
                print("Retrieved all MAC and face data from the database.")

                bluetoothResult = {}

                print("Starting Bluetooth scan thread.")
                bluetoothThread = threading.Thread(
                    target=lambda: bluetoothResult.update({"result": main()})
                )
                bluetoothThread.start()
                bluetoothThread.join()
                print("Bluetooth scan completed.")

                bluetoothDevices = bluetoothResult.get("result", {})
                print(f"Bluetooth devices found: {bluetoothDevices}")

                userInfo = recognitionAndLogin(cameraHandler, allData, bluetoothDevices)
                print("Recognition and login process completed.")

                if userInfo:
                    macAddress = userInfo.get("MAC Address")
                    print(f"User recognized with MAC Address: {macAddress}")
                    setMacAddress(macAddress)  # Save macAddress via callback

                    operation = {
                        "operation": "getRecommendation",
                        "macAddress": macAddress,
                    }
                    print("Fetching recommendations for the user.")
                    recommendations = handleDatabaseOperation(
                        json.dumps(operation)
                    ).get("recommendations")
                    print(f"Recommendations fetched: {recommendations}")

                    response = {"operation": "Recommendations", "data": recommendations}
                    print("Sending recommendations to client.")
                    clientSocket.sendall(json.dumps(response).encode("utf-8"))

            elif operation == "Page":
                print("Operation 'Page' initiated.")
                # Process "Page" operation (if applicable)
                allEmotions = []
                while not cameraHandler.outputQueue.empty():
                    emotion = cameraHandler.outputQueue.get()
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
                print(f"Emotions detected: {allEmotions}")
        else:
            print("No data received from client.")
