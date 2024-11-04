from firestore_connection import FirestoreConnection
from datetime import datetime
import json
from google.cloud import firestore  # Ensure this import is present

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
                
                # Calculate the number of posts by counting references in the 'Posts' field
                posts_count = len(tuio_data.get("Posts", []))  # Get length of 'Posts' array

                # Format Firestore timestamp to string for createdAt
                createdAt = tuio_data.get('createdAt')
                if createdAt:
                    createdAt = createdAt.isoformat() if hasattr(createdAt, 'isoformat') else str(createdAt)

                # Prepare filtered TUIO data to be sent to the client
                tuio_data_filtered = {
                    'tuio_id': doc.id,
                    'description': tuio_data.get('description', ''),
                    'createdAt': createdAt,
                    'posts_count': posts_count  # Calculated posts count
                }
                tuios_list.append(tuio_data_filtered)

            return tuios_list
        except Exception as e:
            print(f"An error occurred while reading TUIOs: {e}")
            return None

    def get_posts_by_tuio(self, tuio_id):
        try:
            tuio_ref = self.db.collection("TUIO").document(tuio_id)
            tuio_doc = tuio_ref.get()

            if not tuio_doc.exists:
                print(f"No TUIO document found with ID {tuio_id}.")
                return None

            tuio_data = tuio_doc.to_dict()

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

    def create_post(self, post_data):
        try:
            post_ref = self.db.collection('Posts').document()  # Auto-generate document ID
            post_data['createdAt'] = datetime.now().isoformat()
            post_ref.set(post_data)
            return f"Post created with ID: {post_ref.id}"
        except Exception as e:
            return f"Error creating post: {e}"

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

    def update_post(self, post_id, new_data):
        try:
            post_ref = self.db.collection('Posts').document(post_id)
            new_data['updatedAt'] = datetime.now().isoformat()
            post_ref.update(new_data)
            return f"Post {post_id} updated successfully."
        except Exception as e:
            return f"Error updating post: {e}"

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
        if doc.exists:
            tuio_data = doc.to_dict()
            if not tuio_data.get('isDeleted', False):
                posts_data = []
                if 'Posts' in tuio_data:
                    for post_ref in tuio_data['Posts']:
                        post_doc = post_ref.get()
                        if post_doc.exists:
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

    def create_user_document(self, doc_id, data):
        try:
            current_time = datetime.now()
            data['createdAt'] = current_time
            data['updatedAt'] = current_time
            data['isDeleted'] = False
            
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
            users_ref = self.db.collection("user").where('isDeleted', '==', False)
            docs = users_ref.stream()

            user_list = []
            for doc in docs:
                user_data = doc.to_dict()
                user_data['user_id'] = doc.id  # Add document ID as 'user_id'
                
                user_data = self._convert_to_serializable(user_data)
                
                user_list.append(user_data)

            return user_list
        except Exception as e:
            print(f"An error occurred while reading users: {e}")
            return None

    def read_all_posts(self):
        try:
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
            if hasattr(value, "isoformat"):
                data[key] = value.isoformat()  # Convert datetime to ISO format
            elif isinstance(value, firestore.DocumentReference):
                data[key] = value.id  # Convert DocumentReference to document ID
            elif isinstance(value, bytes):
                data[key] = value.decode('utf-8')  # Decode bytes to a UTF-8 string
        return data

    def list_collections_and_documents(self):
        try:
            collections = self.db.collections()
            print("Collections and their contents in the Firestore database:")

            for collection in collections:
                print(f"\nCollection: {collection.id}")
                docs = collection.stream()
                for doc in docs:
                    print(f"  Document ID: {doc.id}")
                    print(f"  Data: {doc.to_dict()}")
                    
        except Exception as e:
            print(f"Error retrieving collections or documents: {e}")
