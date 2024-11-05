using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using TUIO;

namespace Tarbita3._0
{
    public partial class Register : Form, TuioListener
    {
        List<User> posts = new List<User>();
        private TuioClient client;
        private Dictionary<long, TuioObject> objectList;

        public class User
        {
            public string CreatedAt { get; set; }
            public string Content { get; set; }
            public string PostId { get; set; }

            public override string ToString()
            {
                return $"Created At: {CreatedAt}\nContent: {Content}\nPost ID: {PostId}";
            }
        }

        public Register()
        {
            InitializeComponent();
            this.FormClosing += Register_FormClosing;

            // Initialize TUIO client
            client = new TuioClient(3333); // Replace 3333 with the appropriate port
            client.addTuioListener(this);
            client.connect();

            objectList = new Dictionary<long, TuioObject>();
        }

        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.removeTuioListener(this);
            client.disconnect();
            System.Windows.Forms.Application.Exit();
        }

        private Timer loginTimer;

        private void Register_Load(object sender, EventArgs e)
        {
            MaximizeWindow(sender, e);

            // Get the path of the image in the Debug folder
            string imagePath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "login.png");

            // Set the background image of the form
            this.BackgroundImage = System.Drawing.Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            loginTimer = new Timer();
            loginTimer.Interval = 2000; // 2 seconds delay
            loginTimer.Tick += LoginTimer_Tick; // Subscribe to the Tick event
            loginTimer.Start(); // Start the timer

        }
        private void LoginTimer_Tick(object sender, EventArgs e)
        {
            // Stop the timer so it only triggers once
            loginTimer.Stop();

            // Call the doLogin method
            doLogin(sender, e);
        }
        private void MaximizeWindow(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }
        private void HandleAdminUsersAndBluetoothDevices()
        {
            // Get the list of all users
            JObject response = PerformCRUDOperation("read_all_users", null);
            if (response["data"] is JArray dataArray && dataArray.Count > 0)
            {
                // Extract MAC addresses of users
                var existingMacAddresses = dataArray
                    .Where(user => !string.IsNullOrEmpty(user["MAC"]?.ToString()))
                    .Select(user => user["MAC"]?.ToString())
                    .ToList();

                // Discover Bluetooth devices
                JObject responsebluzdevices = PerformCRUDOperation("discover_bluetooth_devices", null);
                MessageBox.Show("" + responsebluzdevices);
                if (responsebluzdevices["data"] is JArray devicesArray && devicesArray.Count > 0)
                {
                    // Optionally clear previous device labels (if you want to reset before adding new ones)
                    foreach (Control ctrl in this.Controls.OfType<Label>().ToArray())
                    {
                        this.Controls.Remove(ctrl);
                    }

                    // Create labels for each device whose MAC address is not in the database
                    // Starting vertical position for the first label
                    int startingY = 20; // Adjust as needed to position the list on the form

                    // Loop through each device and create a label for it
                    foreach (var device in devicesArray)
                    {
                        string deviceMac = device["mac_address"]?.ToString();
                        if (!existingMacAddresses.Contains(deviceMac))
                        {
                            Label deviceLabel = new Label
                            {
                                Text = $"Is your Device name: {device["device_name"]}?",
                                AutoSize = true,
                                Cursor = Cursors.Hand, // Change cursor to indicate clickable
                                Tag = deviceMac, // Store MAC address for use later
                                Left = 10, // Horizontal position, adjust if needed
                                Top = startingY // Set vertical position for each label
                            };

                            // Add Click event handler for the label
                            deviceLabel.Click += (sender, e) =>
                            {
                                // Create user with empty MAC address
                                CreateUser(deviceMac, device["device_name"]?.ToString());
                                // Start the application
                                string exePath = Path.Combine(Application.StartupPath, @"..\..\..\..\TUIO11_NET-master\bin\Debug\TuioDemo.exe");
                                StartTuioDemo(exePath);
                                this.Close();
                            };

                            // Add the label to the form
                            this.Controls.Add(deviceLabel);

                            // Update startingY for the next label to appear below the previous one
                            startingY += deviceLabel.Height + 5; // Adding a gap of 5 pixels between labels
                        }
                    }

                }
                else
                {
                    MessageBox.Show("No Bluetooth devices found.");
                }
            }
            else
            {
                MessageBox.Show("Can't reach the specific value or no users found.");
            }
        }


        private void CreateUser(string macAddress, string deviceName)
        {
            var createResponse = PerformCRUDOperation("create_user", new
            {
                user_id = deviceName, // Use an appropriate user ID as needed
                data = new
                {
                    MAC = macAddress, // Set MAC address to the one passed in
                    username = deviceName, // Use device name as username for demonstration
                    role="user"
                }
            });

            if (createResponse["Error"] != null)
            {
                MessageBox.Show("Failed to create user: " + createResponse["Error"].ToString(), "Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("User created successfully.", "Create Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

  


        private void StartTuioDemo(string exePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                };

                Process process = Process.Start(startInfo);
                process.WaitForExit();

                string errors = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(errors))
                {
                    MessageBox.Show("Error: " + errors, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("TuioDemo.exe has finished running.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start the application: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        private void doLogin(object sender, EventArgs e)
        {
  
            HandleAdminUsersAndBluetoothDevices();


        }

        private void label5_Click(object sender, EventArgs e)
        {
            this.Close();
            //new introForm1().Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Add functionality here if needed
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle password visibility or other logic here
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Implement registration logic or input validation here
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Implement button click logic or reset fields here
        }

        // TUIO listener methods
        public void addTuioObject(TuioObject o)
        {
            lock (objectList)
            {
                objectList.Add(o.SessionID, o);
            }

            if (o.SymbolID == 1)
            {
                //MessageBox.Show("TUIO object with SymbolID 1 detected!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                TUIOMouse(o.X, o.Y); // Initial mouse move when object is detected
            }

            // Define a threshold for how close the TUIO object needs to be to the cursor
            int threshold = 400;

            if (o.SymbolID == 10)
            {
                int deltaX = Math.Abs((int)(o.X * Screen.PrimaryScreen.Bounds.Width) - Cursor.Position.X);
                int deltaY = Math.Abs((int)(o.Y * Screen.PrimaryScreen.Bounds.Height) - Cursor.Position.Y);

                if (deltaX <= threshold && deltaY <= threshold)
                {
                    // If the TUIO object is within the defined range, simulate a left mouse click
                    LeftMouseClick((int)(o.X * Screen.PrimaryScreen.Bounds.Width), (int)(o.Y * Screen.PrimaryScreen.Bounds.Height));
                }
            }
        }

        public void updateTuioObject(TuioObject o)
        {
            if (o.SymbolID == 1)
            {
                // Move the mouse to the new position of the TUIO object
                TUIOMouse(o.X, o.Y);
            }
        }

        public void removeTuioObject(TuioObject o)
        {
            lock (objectList)
            {
                objectList.Remove(o.SessionID);
            }
        }

        public void addTuioCursor(TuioCursor c) { }
        public void updateTuioCursor(TuioCursor c) { }
        public void removeTuioCursor(TuioCursor c) { }
        public void addTuioBlob(TuioBlob b) { }
        public void updateTuioBlob(TuioBlob b) { }
        public void removeTuioBlob(TuioBlob b) { }
        public void refresh(TuioTime frameTime) { }

        private void SendMessageToServer(TcpClient client, string message)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message: " + ex.Message);
            }
        }

        private JObject ReceiveMessageFromServer(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4026];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                if (IsValidJson(response))
                {
                    if (response.StartsWith("["))
                    {
                        JArray jsonArray = JArray.Parse(response);
                        return new JObject { { "data", jsonArray } };
                    }
                    else
                    {
                        return JObject.Parse(response);
                    }
                }
                else
                {
                    return new JObject { { "Error", "Received data is not valid JSON." } };
                }
            }
            catch (Exception ex)
            {
                return new JObject { { "Error", "Error receiving message: " + ex.Message } };
            }
        }

        private bool IsValidJson(string response)
        {
            response = response.Trim();
            return (response.StartsWith("{") && response.EndsWith("}")) ||
                   (response.StartsWith("[") && response.EndsWith("]"));
        }

        private JObject PerformCRUDOperation(string operation, object data)
        {
            string serverIp = "192.168.1.7";
            int serverPort = 9001;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(serverIp, serverPort);

                    var request = new
                    {
                        operation = operation,
                        data = data
                    };
                    string jsonMessage = JsonConvert.SerializeObject(request);

                    SendMessageToServer(client, jsonMessage);
                    JObject response = ReceiveMessageFromServer(client);
                    client.Close();

                    return response;
                }
            }
            catch (Exception ex)
            {
                return new JObject { { "Error", "Error performing CRUD operation: " + ex.Message } };
            }
        }
        // Importing the mouse_event function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);


        // Constants for mouse events
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        static void TUIOMouse(float x , float y)
        {
            // Map TUIO coordinates to screen coordinates as needed
            int screenX = (int)(x * Screen.PrimaryScreen.Bounds.Width);
            int screenY = (int)(y * Screen.PrimaryScreen.Bounds.Height);

            // Move the mouse cursor to the calculated screen position
            Cursor.Position = new System.Drawing.Point(screenX, screenY);

            // Simulate a left mouse button click
            
        }

        private static void LeftMouseClick(float x, float y)
        {
            // Simulate mouse down and mouse up events to perform a click
            MessageBox.Show("TUIO object with SymbolID 1 detected!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
        }
    }
}
