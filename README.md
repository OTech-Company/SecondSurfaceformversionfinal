# Smart Ordering Table using Hand Gestures and TUIO

## Overview
This project implements a **smart ordering table** that allows users to interact with a digital menu using **hand gestures** and **TUIO touch inputs**. It leverages **socket communication** to connect a Python-based hand gesture recognition system (using CNN) and TUIO signals to a **C# application**.

The project is designed to enhance ordering experiences in restaurants by enabling **gesture-based interactions** and **TUIO-based touch inputs**. The system ensures real-time processing of user inputs and communicates via sockets to a client application.

---
## Features
- **Hand Gesture Recognition:** Uses a **CNN-based** model to recognize hand gestures.
- **TUIO Touch Input Processing:** Detects touch interactions via **TUIO protocol**.
- **Socket Communication:** Python sends recognized gestures and TUIO events to the C# application.
- **Food Detection (YOLO - not in this repo):** YOLO-based food detection is used in a separate Unity AR project.
- **Smart Table Interface:** Users can interact with the menu in an intuitive and futuristic manner.
- **Real-time Order Processing:** Seamless data transmission between gesture recognition and ordering system.
- **Cart Management:** Users can modify their order items dynamically with gestures.
- **Day/Night UI Mode:** Adjusts the interface based on ambient lighting.

---
## System Architecture
1. **Hand Gesture Recognition:**
   - Processes camera feed and detects gestures using CNN.
   - Outputs recognized gestures as commands.
   
2. **TUIO Touch Input Processing:**
   - Detects specific TUIO events that map to predefined commands.
   
3. **Socket Communication:**
   - The Python script runs socket servers that transmit data to the C# application.
   - IP address and port configuration is required before execution.
   
4. **C# Interface:**
   - Receives and processes socket data.
   - Displays an interactive menu with ordering options.

---
## Installation & Setup
### 1. Prerequisites
- Python 3.7+
- OpenCV
- TensorFlow/Keras (for CNN-based hand gesture recognition)
- PyTUIO (for TUIO processing)
- Unity & C# (for UI implementation)
- .NET Framework

### 2. Clone the Repository
```sh
 git clone git@github.com:OTech-Company/SecondSurfaceformversionfinal.git
 cd SecondSurfaceformversionfinal
```

### 3. Install Dependencies
```sh
 pip install -r requirements.txt
```

### 4. Run the Python Socket Server
Ensure all socket connections are established **before running the C# application**.

```sh
 python socket_server.py
```

### 5. Run the C# Client Application
- Configure the correct **IP address** and **port** in the C# application.
- Start the Unity/C# project.

---
## Usage
- **Start the Python server** first to handle gesture and TUIO input processing.
- **Launch the C# application** to display the interactive menu and receive gesture-based inputs.
- Use **hand gestures or TUIO objects** to navigate the menu, add items to the cart, and confirm orders.
- The **cart dynamically updates** based on user selections.
- **Checkout** by performing the relevant gesture or using TUIO signals.

---
## TUIO Mappings
The following **TUIO signals** are mapped to ordering actions:

| TUIO Signal | Action |
|------------|--------|
| `50` | Add to cart |
| `51` | Remove item |
| `52` | Confirm order |
| `53` | Cancel order |
| `54` | Increase quantity |
| `55` | Decrease quantity |
| `60` | Open Lunch Menu |
| `61` | Open Custom Menu |
| `62` | Open Dessert Menu |
| `90` | Select first menu item |
| `91` | Select second menu item |
| `92` | Select third menu item |
| `93` | Select fourth menu item |
| `102` | Proceed to Checkout Confirmation |
| `110` | Adjust Quantity in Cart |
| `112` | Scroll Through Cart Items |
| `136` | Select Recommended Item 1 |
| `137` | Select Recommended Item 2 |
| `138` | Select Recommended Item 3 |
| `139` | Select Recommended Item 4 |

---
## Hand Gesture Controls
Hand gesture mappings will be illustrated in the **attached image** in the repository.

---
## Interface
The **C# interface** displays a **digital menu** where users can:
- Browse food items.
- Add or remove items using gestures or TUIO touch inputs.
- Confirm or cancel orders.
- View real-time responses to gestures and TUIO actions.
- Toggle between **light and dark mode** depending on ambient conditions.

---
## Contributing
Contributions are welcome! To contribute:
1. **Fork** the repository.
2. **Create** a new branch for your feature (`git checkout -b feature-branch`).
3. **Commit** your changes (`git commit -m 'Add new feature'`).
4. **Push** the branch (`git push origin feature-branch`).
5. **Open** a pull request.

---
## Issues & Bug Reporting
If you encounter issues or have feature requests, please create an issue on GitHub under the **Issues** tab.

---
## License
This project is licensed under the **MIT License**.

---
## Contact
For any inquiries or collaboration, please reach out via **[your email or GitHub profile]**.

---
## Repository Link
🔗 [GitHub Repository](https://github.com/OTech-Company/SecondSurfaceformversionfinal)

