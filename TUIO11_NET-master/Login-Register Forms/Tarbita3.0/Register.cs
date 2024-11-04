using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private void Register_Load(object sender, EventArgs e)
        {
            MaximizeWindow(sender, e);

            // Get the path of the image in the Debug folder
            string imagePath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "login.png");

            // Set the background image of the form
            this.BackgroundImage = System.Drawing.Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // Call readAllUsers here
            readAllUsers(sender, e);
        }

        private void MaximizeWindow(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void readAllUsers(object sender, EventArgs e)
        {
            JObject response = PerformCRUDOperation("read_all_users", null);

            if (response["data"] is JArray dataArray && dataArray.Count > 0)
            {
                string role = dataArray[0]["role"].ToString();
                MessageBox.Show(role);
                if (role == "user")
                {
                    string exePath = Path.Combine(Application.StartupPath, @"..\..\..\..\TUIO11_NET-master\bin\Debug\TuioDemo.exe");
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            WorkingDirectory = Path.GetDirectoryName(exePath), // Set the correct working directory
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
                else 
                {
                    
                }
                
            }
            else
            {
                MessageBox.Show("Can't reach the specific value");
            }

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
                MessageBox.Show("TUIO object with SymbolID 1 detected!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
               

            }
        }

        public void updateTuioObject(TuioObject o)
        {
            // Handle TUIO object updates if needed
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
    }
}
