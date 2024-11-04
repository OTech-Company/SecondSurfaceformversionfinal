### all imports
import socket
import firebase_admin
from firebase_admin import credentials, firestore
from threading import Lock, Thread
import json
from datetime import datetime

### connection|
class FirestoreConnection:
    _instance = None
    _lock = Lock()

    def __new__(cls, *args, **kwargs):
        if not cls._instance:
            with cls._lock:
                if not cls._instance:
                    cls._instance = super(FirestoreConnection, cls).__new__(cls)
                    cls._instance._initialize_connection(*args, **kwargs)
        return cls._instance

    def _initialize_connection(self, service_account_key: str):
        if not firebase_admin._apps:
            cred = credentials.Certificate(service_account_key)
            firebase_admin.initialize_app(cred)
            self.db = firestore.client()
            print("Connected to Firestore")
        else:
            self.db = firestore.client()
            print("Firestore connection reused")

    def get_db(self):
        return self.db
    
    def get_all_posts(self):
        posts_ref = self.db.collection('posts')
        try:
            docs = posts_ref.stream()
            posts_list = []
            for doc in docs:
                post_data = doc.to_dict()
                post_data['id'] = doc.id
                posts_list.append(post_data)
            return posts_list
        except Exception as e:
            print(f"An error occurred while reading posts: {e}")
            return None

### CRUD Design
class FirestoreCRUD:
    def __init__(self):
        self.db = FirestoreConnection("file.json").get_db()
    
    def read_all_tuios(self):
        try:
            tuios_ref = self.db.collection("TUIO").where('isDeleted', '==', False)
            docs = tuios_ref.stream()
            tuios_list = [{'tuio_id': doc.id, 'description': doc.to_dict().get('description', '')} for doc in docs]
            return tuios_list
        except Exception as e:
            print(f"An error occurred while reading TUIOs: {e}")
            return None

    # CRUD operations for posts, tuios, and users as in original code

### constructing sockets
crud = FirestoreCRUD()
server_ip = '192.168.20.13'
port = 9001
soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
soc.bind((server_ip, port))
soc.listen(5)
print(f"Server is running on {server_ip}:{port} and waiting for connections...")

### Handling Client
def handle_client(client_socket):
    try:
        data = client_socket.recv(1024).decode('utf-8')
        response = "empty"
        if data:
            request = json.loads(data)
            operation = request.get('operation')
            content = request.get('data')
            print(operation)
            match operation:
                case 'create_post':
                    response = crud.create_post(content)
                case 'read_post':
                    post_id = content.get('post_id')
                    response = crud.read_post(post_id)
                case 'delete_post':
                    post_id = content.get('post_id')
                    response = crud.delete_post(post_id)
                case 'update_post':
                    post_id = content.get('post_id')
                    response = crud.update_post(post_id)
                # other cases as per original code
                case _:
                    response = "Unknown operation requested."
            client_socket.send(response.encode('utf-8'))
    except Exception as e:
        error_message = f"Error handling client request: {e}"
        print(error_message)
        client_socket.send(error_message.encode('utf-8'))
    finally:
        client_socket.close()

while True:
    client_socket, client_address = soc.accept()
    print(f"Connected to client at {client_address}")
    client_handler = Thread(target=handle_client, args=(client_socket,))
    client_handler.start()
