import json
import socket


def parseJSON(data):
    """
    Parses a JSON string into a Python dictionary with error handling.
    :param data: JSON string.
    :return: Parsed dictionary, or None if parsing fails.
    """
    try:
        return json.loads(data)
    except json.JSONDecodeError:
        print("Error decoding JSON data.")
        return None


def loadJSON(filePath):
    """
    Loads JSON data from a file with error handling.
    If the file doesn't exist or is corrupted, returns an empty dictionary.

    :param filePath: Path to the JSON file.
    :return: Dictionary with the loaded JSON data, or an empty dictionary.
    """
    try:
        with open(filePath, "r") as file:
            data = file.read()
            return parseJSON(data) or {}
    except FileNotFoundError:
        print(f"Error: The file '{filePath}' was not found.")
        return {}
    except IOError as e:
        print(f"Error reading the file '{filePath}': {e}")
        return {}


def saveJSON(filePath, data):
    """
    Saves data to a JSON file.

    :param filePath: Path to the JSON file.
    :param data: Dictionary to save as JSON.
    """
    try:
        with open(filePath, "w") as file:
            json.dump(data, file, indent=4)
        print(f"Data saved successfully to '{filePath}'.")
    except IOError as e:
        print(f"Error writing to file '{filePath}': {e}")


def getPublicIP():
    """
    Retrieves the public IP address of the machine by connecting to an external server.
    :return: Local IP address as a string, or an error message.
    """
    try:
        hostname = "8.8.8.8"
        port = 80
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
            s.connect((hostname, port))
            localIP = s.getsockname()[0]
        print(localIP)
        return localIP
    except Exception as e:
        print(f"Error fetching local IP: {e}")
        return None


def addConnectionInfo(fileName="Server/config.json"):
    """
    Updates the IP and port fields in the configuration file.
    If the file doesn't exist, it creates a new one with default values.

    :param fileName: Path to the JSON configuration file.
    """
    ip = getPublicIP()
    port = 1010

    # Load existing configuration or start fresh
    configData = loadJSON(fileName)
    server = configData.get("server")
    # Update the IP and port
    server["IP"] = ip
    server["port"] = port

    # Save the updated configuration
    saveJSON(fileName, configData)


def updateConfigFile(newData, filePath="Server/config.json"):
    """
    Updates the JSON config file by appending new data.
    If the file doesn't exist, it creates one.

    :param newData: Dictionary containing the data to add.
    :param filePath: Path to the JSON config file.
    """
    # Load existing configuration or start fresh
    configData = loadJSON(filePath)

    # Update with new data
    configData.update(newData)

    # Save the updated configuration
    saveJSON(filePath, configData)
