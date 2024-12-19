import os
import sys

root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)
import json
from Database.CRUD import FirestoreCRUD


def handleDatabaseOperation(data):
    # Default response message
    response = {"message": "Unknown operation requested."}
    try:
        if data:
            # Parse the incoming request
            request = json.loads(data)
            operation = request.get("operation")

            # Handle the 'get_all_user_mac_and_face_image' operation
            if operation == "get_all_user_mac_and_face_image":
                # Call the CRUD function to get the macAddress, faceImage, and name of all users
                crud = FirestoreCRUD()
                user_info = crud.get_all_user_mac_face_name()

                # Check if user info is found and send it back
                if user_info:
                    response = {"data": user_info}
                else:
                    response = {"message": "No user data found."}

            # Handle the 'get_recommendation_data' operation
            elif operation == 'get_recommendation_data':
                mac_address = request.get('macAddress')
                if not mac_address:
                    response = {"error": "macAddress is required for this operation."}
                else:
                    # Call the recommendation function
                    crud = FirestoreCRUD()
                    recommendation = crud.get_recommendation_data(mac_address)
                    response = recommendation

            # Handle the 'update_emotions' operation
            elif operation == 'update_emotions':
                mac_address = request.get('macAddress')
                page_id = request.get('page_id')
                expressions = request.get('expressions', [])

                # Validate input
                if not mac_address:
                    response = {"error": "macAddress is required for this operation."}
                elif not page_id or page_id not in [1, 2, 3, 4]:
                    response = {"error": "page_id must be one of 1, 2, 3, or 4."}
                elif not isinstance(expressions, list):
                    response = {"error": "expressions must be an array."}
                else:
                    # Call the update_emotions function with the new parameters
                    crud = FirestoreCRUD()
                    update_response = crud.update_emotions(mac_address, page_id, expressions)
                    response = update_response

    except json.JSONDecodeError:
        response = {"error": "Invalid JSON format."}
        print(json.dumps(response))
    except Exception as e:
        # Handle any errors that occur
        response = {"error": f"Error handling request: {str(e)}"}
        print(json.dumps(response))

    return response