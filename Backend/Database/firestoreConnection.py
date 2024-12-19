import firebase_admin
from threading import Lock
from firebase_admin import credentials, firestore


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
