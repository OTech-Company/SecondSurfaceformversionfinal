import socket
import firebase_admin
from firebase_admin import credentials, firestore
from datetime import datetime
from threading import Lock, Thread
import json

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
        # Initialize the Firebase app with the service account if not already done
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

class FirestoreCRUD:
    def __init__(self):
        self.db = FirestoreConnection("file.json").get_db()  # Path to your Firebase credentials file

    # Create a new post in the "Posts" collection
    def create_post(self, post_data):
        try:
            post_ref = self.db.collection('Posts').document()  # Auto-generate document ID
            post_data['createdAt'] = datetime.now().isoformat()
            post_ref.set(post_data)
            return f"Post created with ID: {post_ref.id}"
        except Exception as e:
            return f"Error creating post: {e}"

    # Read a post by ID
    def read_post(self, post_id):
        try:
            post_ref = self.db.collection('Posts').document(post_id)
            doc = post_ref.get()
            if doc.exists:
                return json.dumps(doc.to_dict())  # Convert Firestore document to JSON
            else:
                return "Post not found."
        except Exception as e:
            return f"Error reading post: {e}"

    # Update an existing post
    def update_post(self, post_id, new_data):
        try:
            post_ref = self.db.collection('Posts').document(post_id)
            new_data['updatedAt'] = datetime.now().isoformat()
            post_ref.update(new_data)
            return f"Post {post_id} updated successfully."
        except Exception as e:
            return f"Error updating post: {e}"

    # Delete a post by ID (either soft-delete or hard-delete)
    def delete_post(self, post_id, soft_delete=True):
        try:
            post_ref = self.db.collection('Posts').document(post_id)
            if soft_delete:
                post_ref.update({"isDeleted": True, "updatedAt": datetime.now().isoformat()})
                return f"Post {post_id} marked as deleted."
            else:
                post_ref.delete()
                return f"Post {post_id} deleted successfully."
        except Exception as e:
            return f"Error deleting post: {e}"

    # CRUD for TUIO Collection (handling multiple posts)
    def create_tuio_document(self, doc_id, data, post_ids=None):
        if post_ids:
            post_refs = [self.db.collection("Posts").document(post_id) for post_id in post_ids]
            data['Posts'] = post_refs
        data['isDeleted'] = False
        self.db.collection("TUIO").document(doc_id).set(data)
        print(f"TUIO document {doc_id} created with posts: {post_ids if post_ids else []}")

    def read_tuio_document(self, tuio_id):
        doc_ref = self.db.collection("TUIO").document(tuio_id)
        doc = doc_ref.get()
        if doc.exists():
            tuio_data = doc.to_dict()
            if not tuio_data.get('isDeleted', False):
                posts_data = []
                if 'Posts' in tuio_data:
                    for post_ref in tuio_data['Posts']:
                        post_doc = post_ref.get()
                        if post_doc.exists():
                            post_data_with_id = {
                                'post_id': post_ref.id,
                                'data': post_doc.to_dict()
                            }
                            posts_data.append(post_data_with_id)
                tuio_data['Posts'] = posts_data
                return tuio_data
            else:
                print(f"TUIO document {tuio_id} is marked as deleted.")
        else:
            print(f"No TUIO document found with ID {tuio_id}.")
        return None

    def update_tuio_document(self, tuio_id, updates):
        tuio_ref = self.db.collection("TUIO").document(tuio_id)
        updates['updatedAt'] = datetime.utcnow()
        tuio_ref.update(updates)
        print(f"TUIO document {tuio_id} updated.")

    def delete_tuio_document(self, tuio_id):
        tuio_ref = self.db.collection("TUIO").document(tuio_id)
        tuio_ref.update({'isDeleted': True, 'updatedAt': datetime.utcnow()})
        print(f"TUIO {tuio_id} marked as deleted.")

        tuio_data = self.read_tuio_document(tuio_id)
        if tuio_data and 'Posts' in tuio_data:
            for post in tuio_data['Posts']:
                self.delete_post(post['post_id'], tuio_id)
    
    # CRUD Operations for User Collection
    def create_user_document(self, doc_id, data):
        data['isDeleted'] = False
        self.db.collection("user").document(doc_id).set(data)
        print(f"User {doc_id} created.")
    
    def read_user_document(self, doc_id):
        doc_ref = self.db.collection("user").document(doc_id)
        doc = doc_ref.get()
        if doc.exists():
            user_data = doc.to_dict()
            if not user_data.get('isDeleted', False):
                return user_data
            else:
                print(f"User {doc_id} is marked as deleted.")
        else:
            print(f"No user found with ID {doc_id}.")
        return None

    def update_user_document(self, user_id, updates):
        user_ref = self.db.collection("user").document(user_id)
        updates['updatedAt'] = datetime.utcnow()
        user_ref.update(updates)
        print(f"User {user_id} updated.")

    def delete_user_document(self, doc_id):
        user_ref = self.db.collection("user").document(doc_id)
        user_ref.update({'isDeleted': True, 'updatedAt': datetime.utcnow()})
        print(f"User {doc_id} marked as deleted.")

        posts_ref = self.db.collection("Posts").where('user_id', '==', doc_id).where('isDeleted', '==', False).stream()
        for post in posts_ref:
            self.delete_post(post.id)

# Instantiate the CRUD object
crud = FirestoreCRUD()

# Server details
server_ip = '192.168.1.16'  # Replace with your IP address
port = 9000

# Create a socket object
soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to the IP and port
soc.bind((server_ip, port))

# Enable the server to accept connections (backlog set to 5)
soc.listen(5)

print(f"Server is running on {server_ip}:{port} and waiting for connections...")

def handle_client(client_socket):
    try:
        # Receive data from the client
        data = client_socket.recv(1024).decode('ascii')

        # Process received data (assuming it's a JSON string with 'operation' and 'data')
        if data:
            request = json.loads(data)
            operation = request.get('operation')
            content = request.get('data')

            if operation == 'create_post':
                response = crud.create_post(content)
            elif operation == 'read_post':
                post_id = content.get('post_id')
                response = crud.read_post(post_id)
            elif operation == 'delete_post':
                post_id = content.get('post_id')
                response = crud.delete_post(post_id)
            elif operation == 'update_post':
                post_id = content.get('post_id')
                response = crud.update_post(post_id)
            elif operation == 'create_tuio':
                tuio_id = content.get('tuio_id')
                tuio_data = content.get('data')
                post_ids = content.get('post_ids', [])
                response = crud.create_tuio_document(tuio_id, tuio_data, post_ids)
            elif operation == 'read_tuio':
                tuio_id = content.get('tuio_id')
                response = crud.read_tuio_document(tuio_id)
            elif operation == 'update_tuio':
                tuio_id = content.get('tuio_id')
                updates = content.get('updates', {})
                response = crud.update_tuio_document(tuio_id, updates)
            elif operation == 'delete_tuio':
                tuio_id = content.get('tuio_id')
                response = crud.delete_tuio_document(tuio_id)

            elif operation == 'create_user':
                user_id = content.get('user_id')
                user_data = content.get('data')
                response = crud.create_user_document(user_id, user_data)
            elif operation == 'read_user':
                user_id = content.get('user_id')
                response = crud.read_user_document(user_id)
            elif operation == 'update_user':
                user_id = content.get('user_id')
                updates = content.get('updates', {})
                response = crud.update_user_document(user_id, updates)
            elif operation == 'delete_user':
                user_id = content.get('user_id')
                response = crud.delete_user_document(user_id)
            else:
                response = "Unknown operation requested."

            # Send a response back to the client
            client_socket.send(response.encode('ascii'))

    except Exception as e:
        error_message = f"Error handling client request: {e}"
        print(error_message)
        client_socket.send(error_message.encode('ascii'))

    finally:
        # Close the connection
        client_socket.close()

while True:
    # Wait for a connection
    client_socket, client_address = soc.accept()
    print(f"Connected to client at {client_address}")

    # Handle client request in a separate thread
    client_handler = Thread(target=handle_client, args=(client_socket,))
    client_handler.start()

