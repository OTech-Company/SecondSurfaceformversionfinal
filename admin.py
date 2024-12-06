import firebase_admin
from firebase_admin import credentials, firestore
import streamlit as st
from threading import Lock
import pandas as pd
from collections import Counter
from dateutil import parser  # Import the date parser


# Firestore Connection Singleton Class
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


# Firestore CRUD Operations
class FirestoreCRUD:
    def __init__(self):
        self.db = FirestoreConnection("final.json").get_db()  # Path to your Firebase credentials file

    # Get all meals
    def get_all_meals(self):
        try:
            meals_ref = self.db.collection('Meals')
            meals = meals_ref.stream()

            meal_info = []
            for meal in meals:
                meal_data = meal.to_dict()
                meal_info.append({
                    'id': meal.id,
                    'name': meal_data.get('name'),
                    'price': meal_data.get('price'),
                    'description': meal_data.get('description'),
                    'image': meal_data.get('image'),
                    'material': meal_data.get('material'),
                    'category': meal_data.get('category'),
                    'allergies': meal_data.get('allergies')
                })

            return meal_info if meal_info else None
        except Exception as e:
            print(f"Error fetching meal data: {e}")
            return None


    # Add a new meal
    def add_meal(self, name, price, description, image, material, category, isGlutenFree, isDiabeticFriendly, isHealthy):
        try:
            meals_ref = self.db.collection('Meals')
            meal_data = {
                'name': name,
                'price': price,
                'description': description,
                'image': image,
                'material': material,
                'category': category,
                'allergies': {
                    'isGlutenFree': isGlutenFree,
                    'isDiabeticFriendly': isDiabeticFriendly,
                    'isHealthy': isHealthy
                }
            }
            meals_ref.add(meal_data)
        except Exception as e:
            print(f"Error adding meal: {e}")


    # Update an existing meal
    def update_meal(self, meal_id, name, price, description, image, material, category, isGlutenFree, isDiabeticFriendly, isHealthy):
        try:
            meal_ref = self.db.collection('Meals').document(meal_id)
            meal_data = {
                'name': name,
                'price': price,
                'description': description,
                'image': image,
                'material': material,
                'category': category,
                'allergies': {
                    'isGlutenFree': isGlutenFree,
                    'isDiabeticFriendly': isDiabeticFriendly,
                    'isHealthy': isHealthy
                }
            }
            meal_ref.update(meal_data)
        except Exception as e:
            print(f"Error updating meal: {e}")

    # Delete a meal
    def delete_meal(self, meal_id):
        try:
            meal_ref = self.db.collection('Meals').document(meal_id)
            meal_ref.delete()
        except Exception as e:
            print(f"Error deleting meal: {e}")


# Streamlit UI for Meal Management

# -----------------------------------------------------
# Display all meals
# Display all meals without 'id' and 'image', and with allergies as separate columns
def display_meals():
    firestore = FirestoreCRUD()
    meals = firestore.get_all_meals()

    if meals:
        # Prepare a new list to hold the formatted meal data
        formatted_meals = []
        for meal in meals:
            # Extract the necessary fields
            meal_data = {
                'Name': meal['name'],
                'Price': meal['price'],
                'Category': meal.get('category', 'N/A'),  # Handle missing category
                'Description': meal.get('description', ''),
            }

            # Extract allergies and convert boolean to "Yes"/"No"
            allergies = meal.get('allergies', {})
            if 'isDiabeticFriendly' in allergies:
                meal_data['isDiabeticFriendly'] = "Yes" if allergies.get('isDiabeticFriendly', False) else "No"
            if 'isGlutenFree' in allergies:
                meal_data['isGlutenFree'] = "Yes" if allergies.get('isGlutenFree', False) else "No"
            if 'isHealthy' in allergies:
                meal_data['isHealthy'] = "Yes" if allergies.get('isHealthy', False) else "No"

            formatted_meals.append(meal_data)

        # Convert the list of formatted meals to a DataFrame
        meals_df = pd.DataFrame(formatted_meals)

        # Optionally, reorder columns and drop columns that were not added
        desired_order = ['Name', 'Price', 'Category', 'Description']
        # Dynamically add allergy columns if they exist
        allergy_columns = ['isDiabeticFriendly', 'isGlutenFree', 'isHealthy']
        existing_allergy_columns = [col for col in allergy_columns if col in meals_df.columns]
        desired_order.extend(existing_allergy_columns)
        meals_df = meals_df[desired_order]

        # Display the DataFrame in Streamlit
        st.write("### Meal List")
        st.dataframe(meals_df)
    else:
        st.write("No meals found.")



# Add a new meal with success alert
def add_meal_ui():
    with st.form(key="add_meal_form"):
        st.write("### Add a New Meal")

        # Input fields
        name = st.text_input("Meal Name")
        price = st.number_input("Price", min_value=0.0, format="%.2f")
        description = st.text_area("Description")
        image = st.text_input("Image Path")
        material = st.text_input("Material")
        category = st.selectbox("Category", ["lunch", "breakfast", "desert"])

        # Allergies fields
        isGlutenFree = st.checkbox("Is Gluten Free")
        isDiabeticFriendly = st.checkbox("Is Diabetic Friendly")
        isHealthy = st.checkbox("Is Healthy")

        # Submit button
        submit_button = st.form_submit_button(label="Add Meal")

        if submit_button:
            firestore = FirestoreCRUD()
            # Add the new meal to the Firestore database
            firestore.add_meal(
                name, 
                price, 
                description, 
                image, 
                material, 
                category, 
                isGlutenFree, 
                isDiabeticFriendly, 
                isHealthy
            )

            st.success(f"Meal '{name}' added successfully!")
            st.experimental_rerun()  # Refresh the page to show the newly added meal


def update_meal_ui():
    firestore = FirestoreCRUD()

    # Fetch all meals
    meals = firestore.get_all_meals()

    if not meals:
        st.write("No meals available to update.")
        return

    # Create a dictionary of meal names and their corresponding IDs
    meal_names = {meal['name']: meal['id'] for meal in meals}

    # Allow admin to select the meal by name
    meal_to_update_name = st.selectbox("Select a Meal to Update", list(meal_names.keys()))

    if meal_to_update_name:
        # Get the ID of the selected meal
        meal_id = meal_names[meal_to_update_name]

        # Find the meal data for the selected meal
        meal = next(meal for meal in meals if meal['id'] == meal_id)

        # Form for updating the meal
        with st.form(key="update_meal_form"):
            st.write("### Update Meal")

            # Input fields with pre-filled values from the existing meal
            name = st.text_input("Meal Name", value=meal['name'])
            price = st.number_input("Price", min_value=0.0, value=float(meal['price']), format="%.2f")
            description = st.text_area("Description", value=meal['description'])
            image = st.text_input("Image Path", value=meal['image'])
            material = st.text_input("Material", value=meal['material'])

            # Updated category select box with capitalized options
            category_options = ["Lunch", "Breakfast", "Desert"]  # Capitalized to match Firestore data
            current_category = meal.get('category', "Lunch")      # Default to "Lunch" if not set

            # Handle unexpected category values
            if current_category not in category_options:
                st.warning(f"Category '{current_category}' not recognized. Defaulting to 'Lunch'.")
                current_category = "Lunch"

            category = st.selectbox(
                "Category", 
                category_options, 
                index=category_options.index(current_category)
            )

            # Allergies fields
            isGlutenFree = st.checkbox("Is Gluten Free", value=meal['allergies'].get('isGlutenFree', False))
            isDiabeticFriendly = st.checkbox("Is Diabetic Friendly", value=meal['allergies'].get('isDiabeticFriendly', False))
            isHealthy = st.checkbox("Is Healthy", value=meal['allergies'].get('isHealthy', False))

            # **Add Submit Button Here**
            submit_button = st.form_submit_button(label="Update Meal")  # Added submit button

            if submit_button:
                # Update the meal in Firestore
                firestore.update_meal(
                    meal_id, 
                    name, 
                    price, 
                    description, 
                    image, 
                    material, 
                    category, 
                    isGlutenFree, 
                    isDiabeticFriendly, 
                    isHealthy
                )

                # Display success message
                st.success(f"Meal '{name}' updated successfully!")

                # Optionally, refresh the page
                # st.experimental_rerun()



# Delete a meal based on its name
def delete_meal_ui():
    firestore = FirestoreCRUD()

    # Fetch all meals
    meals = firestore.get_all_meals()

    if not meals:
        st.write("No meals available to delete.")
        return

    # Create a dictionary of meal names and their corresponding IDs
    meal_names = {meal['name']: meal['id'] for meal in meals}

    # Display the meal names in the dropdown
    meal_to_delete_name = st.selectbox("Select a Meal to Delete", list(meal_names.keys()))

    if meal_to_delete_name:
        confirm_delete = st.button(f"Delete Meal '{meal_to_delete_name}'")

        if confirm_delete:
            try:
                # Get the ID corresponding to the selected meal name
                meal_id = meal_names[meal_to_delete_name]
                firestore.delete_meal(meal_id)
                st.success(f"Meal '{meal_to_delete_name}' deleted successfully!")
                st.experimental_rerun()  # Refresh the page to show updated meal list
            except Exception as e:
                st.error(f"Error deleting meal: {e}")


# -----------------------------------------------------


# Streamlit UI for User Management
# Example functions for User Management (these will need to be implemented later)
# Updated view_users Function
def view_users():
    firestore = FirestoreCRUD()

    # Fetch all users from the 'Users' collection
    users_ref = firestore.db.collection('Users')
    users = users_ref.stream()

    # Create a list of user names for selection
    user_names = []
    user_data_map = {}
    for user in users:
        user_data = user.to_dict()
        user_names.append(user_data['name'])
        user_data_map[user_data['name']] = user_data

    # Create a dropdown menu to select a user by name
    selected_user_name = st.selectbox("Select a User", user_names)

    # Fetch the selected user data
    if selected_user_name:
        selected_user = user_data_map[selected_user_name]

        # Display the user details
        st.write(f"### User Details for {selected_user_name}")
        st.write(f"**Name**: {selected_user['name']}")
        st.write(f"**Mac Address**: {selected_user['macAddress']}")
        st.write(f"**Age**: {selected_user['age']}")  # Display Age

        # Display Allergies if present
        if selected_user.get('allergies'):
            # Handle potential trailing spaces in keys
            allergies = {key.strip(): value for key, value in selected_user['allergies'].items()}
            allergy_display = []
            if allergies.get('gluten', False):
                allergy_display.append('Gluten')
            if allergies.get('diabetic', False):
                allergy_display.append('Diabetic')
            if allergy_display:
                st.write(f"**Allergies**: {', '.join(allergy_display)}")  # Display Allergies
            else:
                st.write("**Allergies**: None")
        else:
            st.write("**Allergies**: None")

        # Optionally display the user's face image
        # st.image(f"images/{selected_user['faceImage']}", width=100)  # Ensure the path is correct

        # Now fetch and display the user's orders
        user_orders = get_user_orders(selected_user['macAddress'])

        if user_orders:
            # Convert user orders to a DataFrame for table display
            orders_data = []
            for order in user_orders:
                # Parse the 'Date' string into a datetime object
                try:
                    order_date = parser.parse(order['Date'])
                    formatted_date = order_date.strftime('%Y-%m-%d %H:%M:%S')
                except Exception as e:
                    # If parsing fails, use the original string
                    formatted_date = order['Date']
                    st.warning(f"Failed to parse date for order: {e}")

                order_info = {
                    "Order Date": formatted_date,
                    "Table Number": order['tableNumber'],
                    "Total Price": order['totalPrice'],
                    "Meals": ', '.join([meal for meal in order['meals'].keys()])
                    # "Own Meals": ', '.join([meal for meal in order.get('ownMeal', {}).keys()])
                }
                orders_data.append(order_info)

            # Create DataFrame
            orders_df = pd.DataFrame(orders_data)

            # Display the orders as a table
            st.write("### Orders Made by the User")
            st.dataframe(orders_df)  # This will display the orders in a table format
        else:
            st.write("No orders found for this user.")


# Function to fetch user orders (ensure this is defined correctly)
def get_user_orders(mac_address):
    firestore = FirestoreCRUD()

    try:
        # Fetch the user document based on mac_address
        user_ref = firestore.db.collection('Users').where('macAddress', '==', mac_address).stream()
        user_doc = next(user_ref, None)

        if user_doc:
            user_data = user_doc.to_dict()
            orders_ref = firestore.db.collection('Users').document(user_doc.id).collection('Orders')
            orders = orders_ref.stream()

            # Fetch and return all orders for the selected user
            order_list = []
            for order in orders:
                order_data = order.to_dict()
                order_list.append(order_data)

            return order_list if order_list else None
        else:
            st.error(f"No user found with MAC Address: {mac_address}")
            return None
    except Exception as e:
        st.error(f"Error fetching user orders: {e}")
        return None


# Add User Function
def add_user():
    st.header("Add New User")
    
    # Create input fields for user details
    name = st.text_input("Name")
    age = st.number_input("Age", min_value=0, value=18, step=1)
    mac_address = st.text_input("MAC Address")
    face_image = st.file_uploader("Upload Face Image", type=["jpg", "png", "jpeg"])
    
    # Allergies input (checkbox for each allergy type)
    allergies = {
        'gluten': st.checkbox('Gluten'),
        'diabetic': st.checkbox('Diabetic')
    }
    
    # If the user submits the form, add the user to Firestore
    if st.button("Add User"):
        # Check if the required fields are filled
        if not name or not mac_address or not face_image:
            st.warning("Please fill in all required fields.")
        else:
            try:
                # Convert allergies dictionary to a map (True if the allergy exists, else False)
                allergies_map = {key: value for key, value in allergies.items()}
                
                # Store the face image (you can save it on cloud storage or locally, here just displaying the name for now)
                face_image_path = f"images/{face_image.name}"
                
                # Prepare the user data to save in Firestore
                user_data = {
                    "name": name,
                    "age": age,
                    "macAddress": mac_address,
                    "faceImage": face_image_path,  # Use path for image saving
                    "allergies": allergies_map
                }
                
                # Add user data to Firestore
                FirestoreCRUD().db.collection('Users').add(user_data)
                
                st.success("User added successfully!")
                st.experimental_rerun()
            except Exception as e:
                st.error(f"Error adding user: {e}")


def delete_user():
    st.header("Delete User")
    
    # Initialize Firestore CRUD instance
    firestore_instance = FirestoreCRUD()
    users_ref = firestore_instance.db.collection('Users')
    users = users_ref.stream()

    # Create a dictionary mapping user names to their document IDs
    user_data_map = {}
    for user in users:
        user_data = user.to_dict()
        user_name = user_data.get('name', 'Unnamed User')
        document_id = user.id  # Actual Firestore document ID
        
        # Check for duplicate names
        if user_name in user_data_map:
            st.warning(f"Duplicate name found: {user_name}. Please ensure user names are unique.")
            # Optionally, you can append a unique identifier or handle duplicates as needed
        else:
            user_data_map[user_name] = document_id

    # Get the list of unique user names
    user_names = list(user_data_map.keys())

    if not user_names:
        st.write("No users available to delete.")
        return

    # Create a dropdown to select a user by name
    selected_user_name = st.selectbox("Select a User to Delete", user_names)

    # Fetch the corresponding document ID
    selected_user_id = user_data_map.get(selected_user_name)

    if selected_user_id:
        selected_user = firestore_instance.db.collection('Users').document(selected_user_id).get().to_dict()
        
        if st.button(f"Delete {selected_user_name}"):
            try:
                # Delete the user document from the 'Users' collection
                firestore_instance.db.collection('Users').document(selected_user_id).delete()
                
                # Delete the user's associated orders if any
                orders_ref = firestore_instance.db.collection('Users').document(selected_user_id).collection('Orders')
                orders = orders_ref.stream()
                for order in orders:
                    orders_ref.document(order.id).delete()
                
                st.success(f"User '{selected_user_name}' and their orders have been deleted successfully!")
                st.experimental_rerun()
            except Exception as e:
                st.error(f"Error deleting user '{selected_user_name}': {e}")
    else:
        st.error("Selected user does not exist.")




def update_user():
    st.header("Update User")

    # Fetch all users from the 'Users' collection
    firestore_instance = FirestoreCRUD()
    users_ref = firestore_instance.db.collection('Users')
    users = users_ref.stream()

    # Create a list of user names for selection
    user_names = []
    user_data_map = {}
    for user in users:
        user_data = user.to_dict()
        user_names.append(user_data['name'])
        user_data_map[user_data['name']] = user_data

    # Create a dropdown to select a user by name
    selected_user_name = st.selectbox("Select a User to Update", user_names)

    # If a user is selected, display their details and allow updating
    if selected_user_name:
        selected_user = user_data_map[selected_user_name]
        
        # Display current user details in the form
        with st.form("update_user_form"):
            # Name input
            name = st.text_input("Name", value=selected_user['name'])
            
            # Age input
            age = st.number_input("Age", min_value=0, value=selected_user['age'], step=1)
            
            # MAC Address input (cannot be changed, should be shown for reference)
            mac_address = st.text_input("MAC Address", value=selected_user['macAddress'], disabled=True)
            
            # Face Image input (allowing a new file upload)
            face_image = st.file_uploader("Upload New Face Image", type=["jpg", "png", "jpeg"])

            # Allergies checkboxes (allow modification)
            allergies = {
                'gluten': st.checkbox('Gluten', value=selected_user['allergies'].get('gluten', False)),
                'diabetic': st.checkbox('Diabetic', value=selected_user['allergies'].get('diabetic', False))
            }

            # Submit button
            submit_button = st.form_submit_button("Update User")

            # On form submission, update the user data
            if submit_button:
                if not name or not mac_address:  # Validate input
                    st.warning("Name and MAC Address are required fields.")
                else:
                    try:
                        # Convert allergies dictionary to a map (True if the allergy exists, else False)
                        allergies_map = {key: value for key, value in allergies.items()}

                        # Handle face image upload
                        face_image_path = f"images/{face_image.name}" if face_image else selected_user['faceImage']

                        # Prepare the updated user data
                        updated_user_data = {
                            "name": name,
                            "age": age,
                            "macAddress": mac_address,  # MAC Address remains unchanged
                            "faceImage": face_image_path,  # New face image or existing
                            "allergies": allergies_map
                        }

                        # Update user data in Firestore
                        firestore_instance.db.collection('Users').document(selected_user['macAddress']).update(updated_user_data)
                        
                        st.success(f"User {name} updated successfully!")
                        st.experimental_rerun()
                    except Exception as e:
                        st.error(f"Error updating user: {e}")


# -----------------------------------------------------


# Streamlit UI for Emotion Analysis
# Updated to include Page 4
# Also includes image display based on selected page

# Function to get all user emotions for a specific page
def get_user_emotions(page_num):
    firestore_instance = FirestoreCRUD()
    users_ref = firestore_instance.db.collection('Users')
    users = users_ref.stream()

    # Create a list of emotions for the selected page
    emotions = []

    for user in users:
        user_data = user.to_dict()
        # Access the correct page expressions field
        page_expressions = user_data.get(f'page{page_num}expressions', [])
        emotions.extend(page_expressions)

    return emotions

# Function to create a chart based on emotions
def create_emotion_chart(emotions):
    if emotions:
        # Count the frequency of each emotion
        emotion_count = Counter(emotions)
        
        # Convert the emotion counts to a DataFrame for easy plotting
        emotion_df = pd.DataFrame(emotion_count.items(), columns=['Emotion', 'Count'])
        emotion_df = emotion_df.sort_values(by='Count', ascending=False)

        # Display the bar chart
        st.bar_chart(emotion_df.set_index('Emotion')['Count'])
    else:
        st.warning("No emotion data found for this page.")

# Main function for Emotion Analysis
def emotion_analysis():
    st.header("Emotion Analysis")

    # Define a mapping from page number to image path
    page_images = {
        1: "images/meals.jpg",
        2: "images/ownmeal.jpg",
        3: "images/recommendation.jpg",
        4: "images/checkout.jpg"
    }

    # Create a dropdown menu to select the page
    page = st.selectbox("Select Page for Emotion Analysis", [1, 2, 3, 4])

    # Display the corresponding image
    image_path = page_images.get(page)
    if image_path:
        try:
            st.image(image_path, use_column_width=True)
        except Exception as e:
            st.error(f"Error loading image for Page {page}: {e}")
    else:
        st.warning(f"No image found for Page {page}.")

    # Display the emotion chart for the selected page
    if page in [1, 2, 3, 4]:
        st.subheader(f"Page {page} - Emotion Analysis")
        emotions = get_user_emotions(page)
        create_emotion_chart(emotions)
    else:
        st.warning("Invalid page selected.")


def main():
    st.title("Admin Dashboard")

    # Sidebar navigation
    menu = ["Meal Management", "User Management", "Emotion Analysis"]
    choice = st.sidebar.selectbox("Select Management Section", menu)

    if choice == "Meal Management":
        # Meal management options
        meal_menu = ["View Meals", "Add Meal", "Update Meal", "Delete Meal"]
        meal_choice = st.sidebar.selectbox("Meal Management Options", meal_menu)

        if meal_choice == "View Meals":
            display_meals()  # View meal function
        elif meal_choice == "Add Meal":
            add_meal_ui()  # Add meal function
        elif meal_choice == "Update Meal":
            update_meal_ui()  # Update meal function
        elif meal_choice == "Delete Meal":
            delete_meal_ui()  # Delete meal function

    elif choice == "User Management":
        # User management options
        user_menu = ["View Users", "Add User", "Update User", "Delete User"]
        user_choice = st.sidebar.selectbox("User Management Options", user_menu)

        if user_choice == "View Users":
            view_users()  # View user function
        elif user_choice == "Add User":
            add_user()  # Add user function
        elif user_choice == "Update User":
            update_user()  # Update user function
        elif user_choice == "Delete User":
            delete_user()  # Delete user function

    elif choice == "Emotion Analysis":
        emotion_analysis()


if __name__ == "__main__":
    main()
