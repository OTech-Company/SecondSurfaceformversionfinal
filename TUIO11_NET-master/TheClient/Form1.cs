using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TheClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void AppendToLog(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);  // Append message to a TextBox (txtLog)
        }
        private void SendMessageToServer(TcpClient client, string message)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
                AppendToLog("Message sent to server: " + message);
            }
            catch (Exception ex)
            {
                AppendToLog("Error sending message: " + ex.Message);
            }
        }

        private string ReceiveMessageFromServer(TcpClient client)
        {
           try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return response;
            }
            catch (Exception ex)
            {
                return "Error receiving message: " + ex.Message;
            }
        }
        private void btnConnect_Click_1(object sender, EventArgs e)
        {
           
        }

        private void PerformCRUDOperation(string operation, object data)
        {
            string serverIp = "192.168.1.16";  // Replace with your server's IP address
            int serverPort = 9000;              // Replace with your server's port number

            try
            {
                // Create a TcpClient and connect to the server
                using (TcpClient client = new TcpClient())
                {
                    AppendToLog("Connecting to server...");
                    client.Connect(serverIp, serverPort);
                    AppendToLog("Connected!");

                    // Prepare the JSON message for CRUD operation
                    var request = new
                    {
                        operation = operation,
                        data = data
                    };
                    string jsonMessage = JsonConvert.SerializeObject(request);

                    // Send the request to the server
                    SendMessageToServer(client, jsonMessage);

                    // Receive response from the server
                    string response = ReceiveMessageFromServer(client);
                    AppendToLog("Received response: " + response);

                    // Close the stream and client connection
                    client.Close();
                    AppendToLog("Connection closed.");
                }
            }
            catch (Exception ex)
            {
                AppendToLog("An error occurred: " + ex.Message);
            }
        }

        //-------------------USER CRUD-----------------------
        private void button1_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("read_post", new
            {
                post_id = "BVrimPoB9fRQ1vInAz7A"  // Replace with the actual post ID you want to read
            });
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("update_post", new
            {
                post_id = "BVrimPoB9fRQ1vInAz7A",  // Replace with the actual post ID you want to update
                updates = new
                {
                    text = "Updated Post from C#"
                }
            });
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("create_post", new
            {
                text = "Post from C#",
                user_id = "user1",
                isDeleted = false
            });
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("delete_post", new
            {
                post_id = "BVrimPoB9fRQ1vInAz7A",  // Replace with the actual post ID you want to delete
                soft_delete = true  // You can change this to false if you want a hard delete
            });
        }


        //---------------TUIO CRUD------------------------
        private void button6_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("create_tuio", new
            {
                tuio_id = "tuio1",
                data = new
                {
                    description = "TUIO description from C#"
                },
                post_ids = new[] { "BVrimPoB9fRQ1vInAz7A" }  // Example post IDs to associate
            });
        }

        private void button8_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("read_tuio", new
            {
                tuio_id = "new_tuio_id"
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("delete_tuio", new
            {
                tuio_id = "new_tuio_id"
            });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("update_tuio", new
            {
                tuio_id = "new_tuio_id",
                updates = new
                {
                    description = "Updated TUIO description from C#"
                }
            });
        }
        //----------------USER CRUD ----------------------
        private void button3_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("update_user", new
            {
                user_id = "user123",
                updates = new
                {
                    username = "updated_user_from_csharp"
                }
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("read_user", new
            {
                user_id = "user123"
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PerformCRUDOperation("create_user", new
            {
                user_id = "user123",
                data = new
                {
                    username = "new_user_from_csharp",
                    isDeleted = false
                }
            });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            PerformCRUDOperation("delete_user", new
            {
                user_id = "user123"
            });
        }
    }

}

