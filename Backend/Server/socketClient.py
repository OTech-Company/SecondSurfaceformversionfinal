import json
import socket
from threading import Thread
from configFile import loadJSON, parseJSON


class SocketClient:
    def __init__(self, config_path="Server/config.json"):
        """
        Initializes the client with server configuration from a JSON file.

        :param config_path: Path to the JSON file containing server configuration.
        """
        self.config = loadJSON(config_path)
        self.serverIP = self.config.get("IP")
        self.port = self.config.get("port")
        self.clientSocket = None
        self.running = False

    def connect(self):
        """
        Establishes a connection to the server.
        """
        try:
            self.clientSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.clientSocket.connect((self.serverIP, self.port))
            self.running = True
            print(f"Connected to the server at {self.serverIP}:{self.port}")
        except Exception as e:
            print(f"Error connecting to the server: {e}")
            self.clientSocket = None

    def sendData(self, data):
        """
        Starts a thread to send data to the server in JSON format.

        :param data: Dictionary to send as JSON.
        """
        if self.running:
            Thread(target=self._sendData, args=(data,), daemon=True).start()
        else:
            print("Client is not connected to the server.")

    def _sendData(self, data):
        """
        Sends data to the server in JSON format.

        :param data: Dictionary to send as JSON.
        """
        if self.clientSocket:
            try:
                dataJSON = json.dumps(data)
                self.clientSocket.sendall(dataJSON.encode("utf-8"))
                print("Data sent successfully.")
            except Exception as e:
                print(f"Error sending data: {e}")
        else:
            print("Client is not connected to the server.")

    def receiveData(self, callback=None):
        """
        Starts a thread to receive data from the server in JSON format.

        :param callback: Optional function to process received data.
        """
        if self.running:
            Thread(target=self._receiveData, args=(callback,), daemon=True).start()
        else:
            print("Client is not connected to the server.")

    def _receiveData(self, callback):
        """
        Receives data from the server in JSON format and passes it to a callback.

        :param callback: Function to process the received data.
        """
        if self.clientSocket:
            try:
                while self.running:
                    received = self.clientSocket.recv(1024).decode("utf-8")
                    if not received:
                        break
                    data = parseJSON(received)
                    if data:
                        print("Data received successfully.")
                        if callback:
                            callback(data)
                        else:
                            data
                    else:
                        print("Received invalid JSON.")
            except Exception as e:
                print(f"Error receiving data: {e}")
        else:
            print("Client is not connected to the server.")

    def disconnect(self):
        """
        Closes the connection to the server.
        """
        self.running = False
        if self.clientSocket:
            try:
                self.clientSocket.close()
                print("Disconnected from the server.")
            except Exception as e:
                print(f"Error disconnecting: {e}")
        else:
            print("Client is already disconnected.")
