### all imports
import socket
import firebase_admin
from firebase_admin import credentials, firestore
from threading import Lock, Thread
import json

# New import
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
    
    # New method to read all documents from the 'posts' collection
    def get_all_posts(self):
        posts_ref = db.collection('posts')
        try:
            # Retrieve all documents in the 'posts' collection
            docs = posts_ref.stream()

            posts_list = []
            for doc in docs:
                post_data = doc.to_dict()
                post_data['id'] = doc.id  # Add document ID to the data
                posts_list.append(post_data)

            return posts_list
        except Exception as e:
            print(f"An error occurred while reading posts: {e}")
            return None
### CRUD Design
class FirestoreCRUD:
    def __init__(self):
        self.db = FirestoreConnection("file.json").get_db()  # Path to your Firebase credentials file
    

    def read_all_tuios(self):
        try:
            tuios_ref = self.db.collection("TUIO").where('isDeleted', '==', False)
            docs = tuios_ref.stream()

            tuios_list = []
            for doc in docs:
                tuio_data = doc.to_dict()
                # Extract only the id and description fields
                tuio_data_filtered = {
                    'tuio_id': doc.id,
                    'description': tuio_data.get('description', '')
                }
                tuios_list.append(tuio_data_filtered)

            return tuios_list
        except Exception as e:
            print(f"An error occurred while reading TUIOs: {e}")
            return None


    def get_posts_by_tuio(self, tuio_id):
        try:
            # Fetch the TUIO document by its ID
            tuio_ref = self.db.collection("TUIO").document(tuio_id)
            tuio_doc = tuio_ref.get()

            if not tuio_doc.exists:
                print(f"No TUIO document found with ID {tuio_id}.")
                return None

            tuio_data = tuio_doc.to_dict()

            # Check if 'Posts' exists in the TUIO document and is not empty
            if 'Posts' in tuio_data and tuio_data['Posts']:
                posts_data = []
                for post_ref in tuio_data['Posts']:
                    post_doc = post_ref.get()
                    if post_doc.exists:
                        post_data = post_doc.to_dict()
                        post_data['post_id'] = post_ref.id  # Include the post ID
                        posts_data.append(post_data)

                return posts_data
            else:
                print(f"TUIO document {tuio_id} has no associated posts.")
                return []

        except Exception as e:
            print(f"An error occurred while retrieving posts for TUIO {tuio_id}: {e}")
            return None
    
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
        try:
            tuio_ref = self.db.collection("TUIO").document(tuio_id)
            doc = tuio_ref.get()
            if doc.exists:
                updates['updatedAt'] = datetime.utcnow().isoformat()
                tuio_ref.update(updates)
                return f"TUIO document {tuio_id} updated successfully."
            else:
                return f"Error: No TUIO document found with ID {tuio_id}."
        except Exception as e:
            return f"Error updating TUIO document {tuio_id}: {e}"

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
        try:
            # Use datetime.now() to set the createdAt and updatedAt fields
            current_time = datetime.now()
            data['createdAt'] = current_time  # Set to the current time when the document is created
            data['updatedAt'] = current_time  # Set to the current time when the document is created
            data['isDeleted'] = False  # For soft-delete logic
            
            # Create the user document
            self.db.collection("user").document(doc_id).set(data)
            return f"User {doc_id} created successfully."
        except Exception as e:
            return f"Error creating user: {e}"
    
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
        try:
            user_ref = self.db.collection("user").document(user_id)
            doc = user_ref.get()
            if doc.exists:
                updates['updatedAt'] = datetime.utcnow().isoformat()
                user_ref.update(updates)
                return f"User {user_id} updated successfully."
            else:
                # Create the document if it doesn't exist
                updates['createdAt'] = datetime.utcnow().isoformat()
                updates['updatedAt'] = datetime.utcnow().isoformat()
                user_ref.set(updates)
                return f"User {user_id} created and updated successfully."
        except Exception as e:
            return f"Error updating user {user_id}: {e}"

    def delete_user_document(self, doc_id):
        user_ref = self.db.collection("user").document(doc_id)
        user_ref.update({'isDeleted': True, 'updatedAt': datetime.utcnow()})
        print(f"User {doc_id} marked as deleted.")

        posts_ref = self.db.collection("Posts").where('user_id', '==', doc_id).where('isDeleted', '==', False).stream()
        for post in posts_ref:
            self.delete_post(post.id)
    
    def read_all_users(self):
        try:
            # Fetch all documents from the 'user' collection where isDeleted is False
            users_ref = self.db.collection("user").where('isDeleted', '==', False)
            docs = users_ref.stream()

            user_list = []
            for doc in docs:
                user_data = doc.to_dict()
                user_data['user_id'] = doc.id  # Add document ID as 'user_id'
                
                # Convert non-serializable fields (e.g., Firestore timestamp) to ISO format
                user_data = self._convert_to_serializable(user_data)
                
                user_list.append(user_data)

            return user_list
        except Exception as e:
            print(f"An error occurred while reading users: {e}")
            return None

    
    def read_all_posts(self):
        try:
            # Fetch all documents from the 'Posts' collection where isDeleted is False
            posts_ref = self.db.collection('Posts').where('isDeleted', '==', False)
            docs = posts_ref.stream()

            posts_list = []
            for doc in docs:
                post_data = doc.to_dict()
                post_data['post_id'] = doc.id  # Add document ID to the data
                posts_list.append(post_data)

            return posts_list
        except Exception as e:
            print(f"An error occurred while reading posts: {e}")
            return None

    def _convert_to_serializable(self, data):
        for key, value in data.items():
            # Check if the value is a datetime-like object (including DatetimeWithNanoseconds)
            if hasattr(value, "isoformat"):
                data[key] = value.isoformat()  # Convert datetime to ISO format
            elif isinstance(value, firestore.DocumentReference):
                data[key] = value.id  # Convert DocumentReference to document ID
            elif isinstance(value, bytes):
                data[key] = value.decode('utf-8')  # Decode bytes to a UTF-8 string
            # Add more type checks as necessary
        return data


    def list_collections_and_documents(self):
        try:
            # Get all top-level collections
            collections = self.db.collections()
            
            print("Collections and their contents in the Firestore database:")

            for collection in collections:
                print(f"\nCollection: {collection.id}")
                
                # Get all documents in the collection
                docs = collection.stream()
                
                for doc in docs:
                    print(f"  Document ID: {doc.id}")
                    print(f"  Data: {doc.to_dict()}")
                    
        except Exception as e:
            print(f"Error retrieving collections or documents: {e}")    
### constructing sockets
# Instantiate the CRUD object
crud = FirestoreCRUD()

# Server details
server_ip = '192.168.1.7'  # Replace with your IP address
port = 9001

# Create a socket object
soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to the IP and port
soc.bind((server_ip, port))

# Enable the server to accept connections (backlog set to 5)
soc.listen(5)

print(f"Server is running on {server_ip}:{port} and waiting for connections...")
### Handeling Client
def handle_client(client_socket):
    try:
        # Receive data from the client
        # data = client_socket.recv(1024).decode('ascii') changed
        data = client_socket.recv(1024).decode('utf-8')
        response = "empty"  # Default response for unknown operations
        # Process received data (assuming it's a JSON string with 'operation' and 'data')
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
            
                case 'create_tuio':
                    tuio_id = content.get('tuio_id')
                    tuio_data = content.get('data')
                    post_ids = content.get('post_ids', [])
                    response = crud.create_tuio_document(tuio_id, tuio_data, post_ids)
            
                case 'read_tuio':
                    tuio_id = content.get('tuio_id')
                    response = crud.read_tuio_document(tuio_id)
            
                case 'update_tuio':
                    tuio_id = content.get('tuio_id')
                    updates = content.get('updates', {})
                    response = crud.update_tuio_document(tuio_id, updates)
            
                case 'delete_tuio':
                    tuio_id = content.get('tuio_id')
                    response = crud.delete_tuio_document(tuio_id)
            
                case 'create_user':
                    user_id = content.get('user_id')
                    user_data = content.get('data')
                    response = crud.create_user_document(user_id, user_data)
            
                case 'read_user':
                    user_id = content.get('user_id')
                    response = crud.read_user_document(user_id)
            
                case 'update_user':
                    user_id = content.get('user_id')
                    updates = content.get('updates', {})
                    response = crud.update_user_document(user_id, updates)
            
                case 'delete_user':
                    user_id = content.get('user_id')
                    response = crud.delete_user_document(user_id)
            
                case 'read_all_users':
                    users = crud.read_all_users()
                    response = users if isinstance(users, str) else json.dumps(users, ensure_ascii=False)
            
                case 'read_all_posts':
                    posts = crud.read_all_posts()
                    response = posts if isinstance(posts, str) else json.dumps(posts, ensure_ascii=False)
            
                case 'read_all_tuios':
                    tuios = crud.read_all_tuios()
                    response = json.dumps(tuios, ensure_ascii=False)
            
                case 'get_posts_by_tuio':
                    tuio_id = content.get('tuio_id')
                    posts = crud.get_posts_by_tuio(tuio_id)
                    response = posts if isinstance(posts, str) else posts  # Handle error if posts is a string
                    response = json.dumps(response)
            
                case _:
                    response = "Unknown operation requested."


            client_socket.send(response.encode('utf-8'))

    except Exception as e:
        error_message = f"Error handling client request: {e}"
        print(error_message)
        client_socket.send(error_message.encode('utf-8'))

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
## sample Creation for Haidy (posts & tuios)
