import os
import sys


root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.append(root_dir)
import json
from datetime import datetime
from google.cloud import firestore  # Ensure this import is present
from Server.configFile import loadJSON
from Database.firestoreConnection import FirestoreConnection


class FirestoreCRUD:
    def __init__(self):
        data = loadJSON("Server/config.json")
        self.db = FirestoreConnection(
            data.get("FIREBASE_SERVICE_ACCOUNT")
        ).get_db()  # Path to your Firebase credentials file

    def get_all_user_mac_face_name(self):
        """
        Fetches all users' macAddress, faceImage, and name from the 'Users' collection.
        """
        try:
            users_ref = self.db.collection("Users")
            users = users_ref.stream()

            user_info = []
            for user in users:
                user_data = user.to_dict()
                # Only add macAddress, faceImage, and name to the response
                user_info.append(
                    {
                        "macAddress": user_data.get("macAddress"),
                        "faceImage": user_data.get("faceImage"),
                        "name": user_data.get("name"),
                    }
                )

            return user_info if user_info else None

        except Exception as e:
            print(f"Error fetching user data: {e}")
            return None

    def get_recommendation_data(self, mac_address: str):
        """
        Generates meal recommendations based on the user's age and allergies.
        Returns a list of four recommendations, each with 'name' and 'image'.
        """
        try:
            users_ref = self.db.collection("Users")
            query = users_ref.where("macAddress", "==", mac_address).limit(1)
            user_docs = query.stream()

            user = None
            user_doc_ref = (
                None  # To hold the document reference for accessing subcollections
            )
            for doc in user_docs:
                user = doc.to_dict()
                user_doc_ref = doc.reference  # Get the document reference
                break

            if not user:
                return {"error": "User not found."}

            age = user.get("age")
            allergies = user.get("allergies", {})
            is_diabetic = allergies.get("diabetic", False)
            is_gluten = allergies.get("gluten", False)

            recommendations = []

            # 1. Retrieve User's Last Order from the 'Orders' Subcollection
            orders_ref = user_doc_ref.collection("Orders")
            orders = (
                orders_ref.order_by("Date", direction=firestore.Query.DESCENDING)
                .limit(1)
                .stream()
            )

            last_order = None
            for order in orders:
                last_order = order.to_dict()
                break  # Only the most recent order is needed

            if last_order:
                meals_in_last_order = last_order.get("meals", {})
                if meals_in_last_order:
                    # Get the meal with the highest quantity in the last order
                    most_ordered_meal_last = max(
                        meals_in_last_order, key=meals_in_last_order.get
                    )
                    # Fetch meal details
                    meal_ref = (
                        self.db.collection("Meals")
                        .where("name", "==", most_ordered_meal_last)
                        .limit(1)
                        .stream()
                    )
                    meal = None
                    for m in meal_ref:
                        meal = m.to_dict()
                        break
                    if meal:
                        recommendations.append(
                            {"name": meal.get("name"), "image": meal.get("image")}
                        )
                    else:
                        recommendations.append(
                            {"name": "Last ordered meal not found.", "image": ""}
                        )
                else:
                    recommendations.append(
                        {"name": "No meals found in the last order.", "image": ""}
                    )
            else:
                recommendations.append(
                    {"name": "No previous orders found.", "image": ""}
                )

            # 2. Age and Allergy-Based Recommendation
            if age <= 12:
                kids_meals = (
                    self.db.collection("Meals").where("category", "==", "Kids").stream()
                )
                kids_meals_list = [meal.to_dict() for meal in kids_meals]
                if kids_meals_list:
                    # Select the most ordered Kids meal
                    # To determine the most ordered, you might need to aggregate orders similar to other categories
                    # For simplicity, we'll select the first one
                    kids_recommendation = {
                        "name": kids_meals_list[0].get("name"),
                        "image": kids_meals_list[0].get("image"),
                    }
                    recommendations.append(kids_recommendation)
                else:
                    recommendations.append(
                        {"name": "No Kids meals available.", "image": ""}
                    )
            else:
                # Determine the group based on allergies
                if is_diabetic:
                    # Query users where 'allergies.diabetic' is True
                    group_query = users_ref.where(
                        "allergies.diabetic", "==", True
                    ).stream()
                elif is_gluten:
                    # Query users where 'allergies.gluten' is True
                    group_query = users_ref.where(
                        "allergies.gluten", "==", True
                    ).stream()
                else:
                    # Query users where both 'allergies.diabetic' and 'allergies.gluten' are False
                    group_query = (
                        users_ref.where("allergies.diabetic", "==", False)
                        .where("allergies.gluten", "==", False)
                        .stream()
                    )

                # Aggregate meal orders
                meal_counts = {}
                for group_user in group_query:
                    group_user_doc_ref = group_user.reference
                    group_orders_ref = group_user_doc_ref.collection("Orders")
                    group_orders = group_orders_ref.stream()
                    for order in group_orders:
                        group_order = order.to_dict()
                        meals = group_order.get("meals", {})
                        for meal_name, count in meals.items():
                            meal_counts[meal_name] = (
                                meal_counts.get(meal_name, 0) + count
                            )

                if meal_counts:
                    # Find the most ordered meal
                    most_ordered_meal = max(meal_counts, key=meal_counts.get)
                    # Fetch meal details
                    meal_ref = (
                        self.db.collection("Meals")
                        .where("name", "==", most_ordered_meal)
                        .limit(1)
                        .stream()
                    )
                    meal = None
                    for m in meal_ref:
                        meal = m.to_dict()
                        break
                    if meal:
                        recommendations.append(
                            {"name": meal.get("name"), "image": meal.get("image")}
                        )
                    else:
                        recommendations.append(
                            {"name": "Recommended meal not found.", "image": ""}
                        )
                else:
                    recommendations.append(
                        {"name": "No data available for recommendation.", "image": ""}
                    )

            # 3. Breakfast Category Recommendation
            breakfast_meals = (
                self.db.collection("Meals")
                .where("category", "==", "Breakfast")
                .stream()
            )
            breakfast_meals_list = [meal.to_dict() for meal in breakfast_meals]

            # Aggregate breakfast meal orders
            breakfast_counts = {}
            for user_doc in users_ref.stream():
                user_data = user_doc.to_dict()
                user_doc_ref_inner = user_doc.reference
                user_orders_ref_inner = user_doc_ref_inner.collection("Orders")
                user_orders = user_orders_ref_inner.stream()
                for order in user_orders:
                    order_data = order.to_dict()
                    meals = order_data.get("meals", {})
                    for meal_name, count in meals.items():
                        # Check if the meal is in Breakfast category
                        if any(
                            meal.get("name") == meal_name
                            and meal.get("category") == "Breakfast"
                            for meal in breakfast_meals_list
                        ):
                            breakfast_counts[meal_name] = (
                                breakfast_counts.get(meal_name, 0) + count
                            )

            if breakfast_counts:
                most_ordered_breakfast = max(breakfast_counts, key=breakfast_counts.get)
                # Fetch meal details
                meal_ref = (
                    self.db.collection("Meals")
                    .where("name", "==", most_ordered_breakfast)
                    .limit(1)
                    .stream()
                )
                meal = None
                for m in meal_ref:
                    meal = m.to_dict()
                    break
                if meal:
                    recommendations.append(
                        {"name": meal.get("name"), "image": meal.get("image")}
                    )
                else:
                    recommendations.append(
                        {"name": "Breakfast meal not found.", "image": ""}
                    )
            else:
                recommendations.append(
                    {"name": "No Breakfast data available.", "image": ""}
                )

            # 4. Lunch Category Recommendation
            lunch_meals = (
                self.db.collection("Meals").where("category", "==", "Lunch").stream()
            )
            lunch_meals_list = [meal.to_dict() for meal in lunch_meals]

            # Aggregate lunch meal orders
            lunch_counts = {}
            for user_doc in users_ref.stream():
                user_data = user_doc.to_dict()
                user_doc_ref_inner = user_doc.reference
                user_orders_ref_inner = user_doc_ref_inner.collection("Orders")
                user_orders = user_orders_ref_inner.stream()
                for order in user_orders:
                    order_data = order.to_dict()
                    meals = order_data.get("meals", {})
                    for meal_name, count in meals.items():
                        # Check if the meal is in Lunch category
                        if any(
                            meal.get("name") == meal_name
                            and meal.get("category") == "Lunch"
                            for meal in lunch_meals_list
                        ):
                            lunch_counts[meal_name] = (
                                lunch_counts.get(meal_name, 0) + count
                            )

            if lunch_counts:
                most_ordered_lunch = max(lunch_counts, key=lunch_counts.get)
                # Fetch meal details
                meal_ref = (
                    self.db.collection("Meals")
                    .where("name", "==", most_ordered_lunch)
                    .limit(1)
                    .stream()
                )
                meal = None
                for m in meal_ref:
                    meal = m.to_dict()
                    break

                if meal:
                    recommendations.append(
                        {"name": meal.get("name"), "image": meal.get("image")}
                    )
                else:
                    recommendations.append(
                        {"name": "Lunch meal not found.", "image": ""}
                    )
            else:
                recommendations.append(
                    {"name": "No Lunch data available.", "image": ""}
                )

            return {"recommendations": recommendations}

        except Exception as e:
            print(f"Error generating recommendations: {e}")
            return {"error": "An error occurred while generating recommendations."}

    def update_emotions(self, mac_address: str, page1expressions: list, page2expressions: list, page3expressions: list):
        """
        Updates the emotions arrays (page1expressions, page2expressions, page3expressions)
        for a user identified by their macAddress by appending new data.

        Parameters:
            mac_address (str): The MAC address of the user.
            page1expressions (list): New expressions for page 1.
            page2expressions (list): New expressions for page 2.
            page3expressions (list): New expressions for page 3.

        Returns:
            dict: A dictionary containing a success message or an error message.
        """
        try:
            users_ref = self.db.collection('Users')
            query = users_ref.where('macAddress', '==', mac_address).limit(1)
            user_docs = query.stream()

            user_doc = None
            for doc in user_docs:
                user_doc = doc
                break

            if not user_doc:
                return {"error": "User not found."}

            user_doc_ref = self.db.collection('Users').document(user_doc.id)

            # Fetch current emotion arrays
            user_data = user_doc.to_dict()
            current_page1 = user_data.get('page1expressions', [])
            current_page2 = user_data.get('page2expressions', [])
            current_page3 = user_data.get('page3expressions', [])

            # Append new expressions
            updated_page1 = current_page1 + page1expressions
            updated_page2 = current_page2 + page2expressions
            updated_page3 = current_page3 + page3expressions

            # Update the Firestore document
            user_doc_ref.update({
                'page1expressions': updated_page1,
                'page2expressions': updated_page2,
                'page3expressions': updated_page3
            })

            return {"message": "Emotions updated successfully."}

        except Exception as e:
            print(f"Error updating emotions: {e}")
            return {"error": "An error occurred while updating emotions."}
