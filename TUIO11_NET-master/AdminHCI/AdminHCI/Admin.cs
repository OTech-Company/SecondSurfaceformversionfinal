using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminHCI
{
    public partial class Admin : Form
    {

        bool sidebarExpand = true;
        private string currentView = "posts"; // Default to posts view
        public Admin()
        {
            InitializeComponent();
            dataGridViewPosts.Dock = DockStyle.Fill;
            dataGridViewPosts.Left = sidebar.Width;
            dataGridViewPosts.Width = this.ClientSize.Width - sidebar.Width;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            sidebarTransition.Start();
        }
        private void ResetButtonStyles()
        {
            // Set default background color and font for all navigation buttons
            Color defaultColor = Color.FromArgb(18, 20, 53);

            pnPosts.BackColor = defaultColor;
            pnPosts.Font = new Font(pnPosts.Font, FontStyle.Regular);

            pnTuio.BackColor = defaultColor;
            pnTuio.Font = new Font(pnTuio.Font, FontStyle.Regular);

            pnUsers.BackColor = defaultColor;
            pnUsers.Font = new Font(pnUsers.Font, FontStyle.Regular);

            PnLogout.BackColor = defaultColor;
            PnLogout.Font = new Font(PnLogout.Font, FontStyle.Regular);
        }

        private void HighlightButton(Button activeButton)
        {
            // Loop through all controls in the sidebar panel
            foreach (Control control in sidebar.Controls)
            {
                if (control is Button button)
                {
                    // Set all buttons to default colors
                    button.ForeColor = Color.White;              
                }
            }

            // Highlight the active button
            activeButton.BackColor = Color.LightBlue;            // Highlight color
            activeButton.ForeColor = Color.Black;                // Font color for highlighted button
            activeButton.Font = new Font(activeButton.Font, FontStyle.Bold); // Bold font style for active button
        }


        private void pnPosts_Click(object sender, EventArgs e)
        {

            ResetButtonStyles();
            HighlightButton(pnPosts);

            currentView = "posts"; // Set current view to posts
            PerformCRUDOperation("read_all_posts", null);
        }

        private void DisplayPostsInGrid(string operation , object data)
        {
            // Clear existing rows and columns in the DataGridView
            dataGridViewPosts.Rows.Clear();
            dataGridViewPosts.Columns.Clear();

            switch (operation)
            {
                case "read_all_posts":
                    // Add columns specific to Posts
                    dataGridViewPosts.Columns.Add("index", "Index");
                    dataGridViewPosts.Columns.Add("post_id", "Post ID");
                    dataGridViewPosts.Columns.Add("text", "Text");
                    dataGridViewPosts.Columns.Add("user_id", "User ID");
                    dataGridViewPosts.Columns.Add("date", "Date");
                    dataGridViewPosts.Columns.Add("time", "Time");

                    // Populate DataGridView with posts
                    var posts = data as List<Post>;
                    if (posts != null)
                    {
                        for (int i = 0; i < posts.Count; i++)
                        {
                            var post = posts[i];
                            DateTime createdAt;
                            if (DateTime.TryParse(post.createdAt, out createdAt))
                            {
                                string date = createdAt.ToString("yyyy-MM-dd");
                                string time = createdAt.ToString("hh:mm:ss tt");
                                if (!post.isDeleted)
                                    dataGridViewPosts.Rows.Add($"Post {i}", post.post_id, post.text, post.user_id, date, time);
                            }
                            else
                            {
                                if(!post.isDeleted)
                                    dataGridViewPosts.Rows.Add($"Post {i}", post.post_id, post.text, post.user_id, "", "");
                            }
                        }
                    }
                    break;

                case "read_all_users":
                    // Add columns specific to Users
                    dataGridViewPosts.Columns.Add("user_id", "User ID");
                    dataGridViewPosts.Columns.Add("name", "Name");
                    dataGridViewPosts.Columns.Add("role", "Role");
                    dataGridViewPosts.Columns.Add("createdAt", "Created At");
                    dataGridViewPosts.Columns.Add("updatedAt", "Updated At");

                    // Populate DataGridView with users
                    var users = data as List<User>;
                    if (users != null)
                    {
                        foreach (var user in users)
                        {
                            DateTime createdAt, updatedAt;
                            string createdDate = DateTime.TryParse(user.createdAt, out createdAt) ? createdAt.ToString("yyyy-MM-dd hh:mm:ss tt") : "";
                            string updatedDate = DateTime.TryParse(user.updatedAt, out updatedAt) ? updatedAt.ToString("yyyy-MM-dd hh:mm:ss tt") : "";
                            dataGridViewPosts.Rows.Add(user.user_id, user.name, user.role, createdDate, updatedDate);
                        }
                    }
                    break;

                case "read_all_tuios":
                    // Add columns specific to TUIO documents
                    dataGridViewPosts.Columns.Add("tuio_id", "TUIO ID");
                    dataGridViewPosts.Columns.Add("description", "Description");
                    dataGridViewPosts.Columns.Add("createdAt", "Created At");
                    dataGridViewPosts.Columns.Add("posts_count", "Number of Posts");

                    var tuios = data as List<TUIO>;
                    if (tuios != null)
                    {
                        foreach (var tuio in tuios)
                        {
                            DateTime createdAt;
                            string createdAtString = DateTime.TryParse(tuio.createdAt, out createdAt) ? createdAt.ToString("yyyy-MM-dd hh:mm:ss tt") : "";
                            dataGridViewPosts.Rows.Add(tuio.tuio_id, tuio.description, createdAtString, tuio.posts_count);
                        }
                    }
                    break;
            }

            // Set each column's AutoSizeMode to Fill so they expand to fit the grid's width
            foreach (DataGridViewColumn column in dataGridViewPosts.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // Adjust row height for better readability
            dataGridViewPosts.RowTemplate.Height = 40;
            dataGridViewPosts.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewPosts.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewPosts.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dataGridViewPosts.DefaultCellStyle.Font = new Font("Arial", 10);
        }


        private void PerformCRUDOperation(string operation, object data)
        {
            string serverIp = "192.168.20.129";  // Replace with your server's IP address
            int serverPort = 9001;               // Replace with your server's port number

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
                    string response = ReceiveMessageFromServer(client);

                    if (response.StartsWith("{"))
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);

                        // Check if response has a 'data' field for list of items
                        if (jsonResponse != null)
                        {
                            if (jsonResponse.data != null && currentView == "tuios")
                            {
                                // Deserialize 'data' into List<TUIO> if in TUIO view
                                var tuios = JsonConvert.DeserializeObject<List<TUIO>>(jsonResponse.data.ToString());
                                DisplayPostsInGrid("read_all_tuios", tuios);
                            }
                            else if (jsonResponse.data != null && currentView == "posts")
                            {
                                // Handle posts deserialization
                                var posts = JsonConvert.DeserializeObject<List<Post>>(jsonResponse.data.ToString());
                                DisplayPostsInGrid("read_all_posts", posts);
                            }
                            else if (jsonResponse.data != null && currentView == "users")
                            {
                                // Handle users deserialization
                                var users = JsonConvert.DeserializeObject<List<User>>(jsonResponse.data.ToString());
                                DisplayPostsInGrid("read_all_users", users);
                            }
                            else if (jsonResponse.message != null)
                            {
                                // Show the message if it's not a data list
                                MessageBox.Show((string)jsonResponse.message);
                            }
                        }
                    }
                    else
                    {
                        // If response is not in JSON format, display it as a raw message
                        MessageBox.Show(response);
                    }

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }


        private void SendMessageToServer(TcpClient client, string message)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
                //AppendToLog("Message sent to server: " + message);
            }
            catch (Exception ex)
            {
                //AppendToLog("Error sending message: " + ex.Message);
            }
        }

        private string ReceiveMessageFromServer(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return response;
            }
            catch (Exception ex)
            {
                return "Error receiving message: " + ex.Message;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void sidebarTransition_Tick(object sender, EventArgs e)
        {
            if (sidebarExpand)
            {
                sidebar.Width -= 10;
                if (sidebar.Width <= 102)
                {
                    sidebarExpand = false;
                    sidebarTransition.Stop();

                    pnPosts.Width = sidebar.Width;
                    // Remove this line for `pnTuio`
                    pnTuio.Width = sidebar.Width;
                    pnUsers.Width = sidebar.Width;
                    PnLogout.Width = sidebar.Width;

                    pnPosts.Text = "";
                    // Remove this line for `pnTuio`
                    pnTuio.Text = "";
                    pnUsers.Text = "";
                    PnLogout.Text = "";
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width >= 200)
                {
                    sidebarExpand = true;
                    sidebarTransition.Stop();

                    pnPosts.Width = sidebar.Width;
                    // Remove this line for `pnTuio`
                    pnTuio.Width = sidebar.Width;
                    pnUsers.Width = sidebar.Width;
                    PnLogout.Width = sidebar.Width;

                    pnPosts.Text = "Posts";
                    // Remove this line for `pnTuio`
                    pnTuio.Text = "TUIO";
                    pnUsers.Text = "Users";
                    PnLogout.Text = "Logout";
                }
            }
            dataGridViewPosts.Dock = DockStyle.Fill;
            dataGridViewPosts.Left = sidebar.Width;
            dataGridViewPosts.Width = this.ClientSize.Width - sidebar.Width;
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void pnUsers_Click(object sender, EventArgs e)
        {
            ResetButtonStyles();
            HighlightButton(pnUsers);

            currentView = "users"; // Set current view to users
            PerformCRUDOperation("read_all_users", null);
        }


        private void PnLogout_Click(object sender, EventArgs e)
        {
            // Path to the login application
            string exePath = Path.Combine(Application.StartupPath, @"..\..\..\..\Login-Register Forms\Tarbita3.0\bin\Debug\Tarbita3.0.exe");

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

                // Start the login process
                Process process = Process.Start(startInfo);
                process.WaitForExit();

                string errors = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(errors))
                {
                    MessageBox.Show("Error: " + errors, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Login application has finished running.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start the application: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Close the current form to return to the login page
            //this.Close(); // Close only this form
            Application.Exit();     // Or, if you want to close the entire application, use Application.Exit();
        }


        private void dataGridViewPosts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void pnTuio_Click(object sender, EventArgs e)
        {
            ResetButtonStyles();
            HighlightButton(pnTuio);

            currentView = "tuios"; // Set current view to TUIOs
            PerformCRUDOperation("read_all_tuios", null);
        }

        private void deletebtn_Click(object sender, EventArgs e)
        {
            // Ensure a row is selected
            if (dataGridViewPosts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an entry to delete.");
                return;
            }

            // Get the selected row
            var selectedRow = dataGridViewPosts.SelectedRows[0];

            if (currentView == "posts")
            {
                // Retrieve the post_id from the selected row
                string postId = selectedRow.Cells["post_id"].Value?.ToString();
                if (!string.IsNullOrEmpty(postId))
                {
                    // Confirm deletion
                    var result = MessageBox.Show("Are you sure you want to delete this post?", "Confirm Delete", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        PerformCRUDOperation("delete_post", new { post_id = postId });
                        MessageBox.Show("Post deleted successfully.");
                        PerformCRUDOperation("read_all_posts", null); // Refresh grid
                    }
                }
            }
            else if (currentView == "users")
            {
                // Retrieve the user_id from the selected row
                string userId = selectedRow.Cells["user_id"].Value?.ToString();
                if (!string.IsNullOrEmpty(userId))
                {
                    // Confirm deletion
                    var result = MessageBox.Show("Are you sure you want to delete this user and all their posts?", "Confirm Delete", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        PerformCRUDOperation("delete_user", new { user_id = userId });
                        MessageBox.Show("User and all associated posts deleted successfully.");
                        PerformCRUDOperation("read_all_users", null); // Refresh grid
                    }
                }
            }
            else if (currentView == "tuios")
            {
                // Retrieve the tuio_id from the selected row
                string tuioId = selectedRow.Cells["tuio_id"].Value?.ToString();
                if (!string.IsNullOrEmpty(tuioId))
                {
                    // Confirm deletion
                    var result = MessageBox.Show("Are you sure you want to delete this TUIO and all its associated posts?", "Confirm Delete", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        PerformCRUDOperation("delete_tuio", new { tuio_id = tuioId });
                        MessageBox.Show("TUIO and all associated posts deleted successfully.");
                        PerformCRUDOperation("read_all_tuios", null); // Refresh grid
                    }
                }
            }
        }

    }

    public class Post
    {
        public string post_id { get; set; }
        public string text { get; set; }
        public string user_id { get; set; }
        public bool isDeleted { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class User
    {
        public string user_id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public bool isDeleted { get; set; }
    }

    public class TUIO
    {
        public string tuio_id { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; } // Store as string for formatting
        public int posts_count { get; set; }
    }

}
