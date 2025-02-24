import pandas as pd
from ultralytics import YOLO
import matplotlib.pyplot as plt
from Server.configFile import updateConfigFile

savePath = "Models/YOLOv11 Trained.pt"

# Step 1: Train the Model
model = YOLO("Models/yolo11n.pt")

# Train the model
results = model.train(data="Code/data.yaml", epochs=100, imgsz=640)

# Save the trained model after the first phase of training
model.save(savePath)

# Update the Configuration file
updateConfigFile({"YOLO": savePath})

# Step 2: Access Metrics
# Extract metrics from results
history = results.results  # Training history (loss, metrics per epoch)

# Convert results to a DataFrame
df = pd.DataFrame(history)

# Save metrics to CSV for future reference
df.to_csv("training_metrics.csv", index=False)
print("Metrics saved to training_metrics.csv")

# Step 3: Plot and Save Metrics

# Plot training and validation loss
plt.figure(figsize=(10, 6))
plt.plot(df["train/box_loss"], label="Train Box Loss")
plt.plot(df["train/obj_loss"], label="Train Object Loss")
plt.plot(df["val/box_loss"], label="Validation Box Loss")
plt.plot(df["val/obj_loss"], label="Validation Object Loss")
plt.xlabel("Epochs")
plt.ylabel("Loss")
plt.title("Training and Validation Loss")
plt.legend()
plt.grid()
plt.savefig("/Images/YOLO Loss.png")  # Save the loss plot
plt.show()

# Plot mAP (mean Average Precision)
plt.figure(figsize=(10, 6))
plt.plot(df["metrics/mAP_0.5"], label="mAP@0.5")
plt.plot(df["metrics/mAP_0.5:0.95"], label="mAP@0.5:0.95")
plt.xlabel("Epochs")
plt.ylabel("mAP")
plt.title("Mean Average Precision (mAP)")
plt.legend()
plt.grid()
plt.savefig("/Images/YOLO mAP.png")  # Save the mAP plot
plt.show()

# Plot Precision and Recall
plt.figure(figsize=(10, 6))
plt.plot(df["metrics/precision"], label="Precision")
plt.plot(df["metrics/recall"], label="Recall")
plt.xlabel("Epochs")
plt.ylabel("Value")
plt.title("Precision and Recall")
plt.legend()
plt.grid()
plt.savefig("/Images/YOLO Precision Recall.png")  # Save the precision-recall plot
plt.show()
