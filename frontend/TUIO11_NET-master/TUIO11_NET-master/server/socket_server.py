import socket
from threading import Thread
from crud import FirestoreCRUD
import json
from config import SERVER_IP, PORT

# Instantiate the CRUD object
crud = FirestoreCRUD()

def handle_client(client_socket):
    try:
        data = client_socket.recv(1024).decode('utf-8')

        if data:
            request = json.loads(data)
            operation = request.get('operation')
            content = request.get('data')

            response = {"message": "Operation completed successfully."}

            if operation == 'create_post':
                response = {"message": crud.create_post(content)}
            elif operation == 'read_post':
                post_id = content.get('post_id')
                response = {"message": crud.read_post(post_id)}
            elif operation == 'delete_post':
                post_id = content.get('post_id')
                response = {"message": crud.delete_post(post_id)}
            elif operation == 'update_post':
                post_id = content.get('post_id')
                response = {"message": crud.update_post(post_id, content.get('new_data', {}))}
            elif operation == 'create_tuio':
                tuio_id = content.get('tuio_id')
                tuio_data = content.get('data')
                post_ids = content.get('post_ids', [])
                response = {"message": crud.create_tuio_document(tuio_id, tuio_data, post_ids)}
            elif operation == 'get_posts_by_tuio':
                tuio_id = content.get('tuio_id')
                posts = crud.get_posts_by_tuio(tuio_id)
                response = posts if isinstance(posts, str) else posts  # Handle error if posts is a string
                response = json.dumps(response)
            elif operation == 'read_tuio':
                tuio_id = content.get('tuio_id')
                response = {"data": crud.read_tuio_document(tuio_id)}
            elif operation == 'update_tuio':
                tuio_id = content.get('tuio_id')
                updates = content.get('updates', {})
                response = {"message": crud.update_tuio_document(tuio_id, updates)}
            elif operation == 'delete_tuio':
                tuio_id = content.get('tuio_id')
                response = {"message": crud.delete_tuio_document(tuio_id)}
            elif operation == 'read_all_tuios':
                tuios = crud.read_all_tuios()
                response = {"data": tuios} if tuios else {"message": "No TUIOs found."}

            elif operation == 'create_user':
                user_id = content.get('user_id')
                user_data = content.get('data')
                response = {"message": crud.create_user_document(user_id, user_data)}
            elif operation == 'read_user':
                user_id = content.get('user_id')
                response = {"data": crud.read_user_document(user_id)}
            elif operation == 'update_user':
                user_id = content.get('user_id')
                updates = content.get('updates', {})
                response = {"message": crud.update_user_document(user_id, updates)}
            elif operation == 'delete_user':
                user_id = content.get('user_id')
                response = {"message": crud.delete_user_document(user_id)}
            elif operation == 'read_all_users':
                users = crud.read_all_users()
                response = {"data": users} if users else {"message": "No users found."}
            elif operation == 'read_all_posts':
                posts = crud.read_all_posts()
                response = {"data": posts} if posts else {"message": "No posts found."}
            else:
                response = {"message": "Unknown operation requested."}

            client_socket.send(json.dumps(response).encode('utf-8'))

    except Exception as e:
        error_message = json.dumps({"error": f"Error handling client request: {str(e)}"})
        print(error_message)
        client_socket.send(error_message.encode('utf-8'))

    finally:
        client_socket.close()

def start_server():
    soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    soc.bind((SERVER_IP, PORT))
    soc.listen(5)
    print(f"Server is running on {SERVER_IP}:{PORT} and waiting for connections...")

    while True:
        client_socket, client_address = soc.accept()
        print(f"Connected to client at {client_address}")
        client_handler = Thread(target=handle_client, args=(client_socket,))
        client_handler.start()
