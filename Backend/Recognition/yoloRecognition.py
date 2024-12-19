import threading


# Threaded function for processing frames with YOLO
class YoloDetectionThread(threading.Thread):
    def __init__(self, frameQueue, outputQueue, model):
        threading.Thread.__init__(self)
        self.frameQueue = frameQueue
        self.outputQueue = outputQueue
        self.model = model
        self.running = True

    def run(self):
        while self.running:
            if not self.frameQueue.empty():
                frame = self.frameQueue.get()
                detections = detect_objects(frame, self.model)
                self.outputQueue.put(detections)

    def stop(self):
        self.running = False


def detect_objects(frame, model):
    results = model(frame)  # Perform inference on the frame

    detections = []
    # Each result corresponds to a single image, hence we loop over results
    for result in results:
        # Extracting the bounding boxes (x, y, width, height), confidences, and labels
        boxes = result.boxes  # Bounding box coordinates and other info
        confidences = boxes.conf  # Confidence scores for each detection
        labels = boxes.cls  # Class labels for each detection

        # Loop through each detection
        for i in range(len(boxes)):
            if confidences[i] > 0.5:  # Filter by confidence threshold
                x, y, w, h = boxes.xywh[i].tolist()  # Extract bounding box coordinates
                label = labels[i].item()  # Extract class label
                conf = confidences[i].item()  # Extract confidence

                # Skip human detections (adjust label number if needed)
                if label == 0:  # Assuming "human" has label 0, modify as needed
                    continue

                detections.append((label, conf, x, y, w, h))

    return detections
