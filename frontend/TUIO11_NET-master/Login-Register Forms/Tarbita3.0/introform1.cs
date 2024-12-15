using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TUIO;

namespace Tarbita3._0
{
    public partial class introform1 : Form, TuioListener
    {
        private TuioClient client;
        private Dictionary<long, TuioObject> objectList;

        public introform1()
        {
            InitializeComponent();
            this.FormClosing += introForm_FormClosing;

            // Initialize TUIO client
            client = new TuioClient(3333); // Replace 3333 with the appropriate port if necessary
            client.addTuioListener(this);
            client.connect();

            objectList = new Dictionary<long, TuioObject>();
        }

        private void introForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.removeTuioListener(this);
            client.disconnect();
            Application.Exit();
        }

        private void MaximizeWindow(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void introForm_Load(object sender, EventArgs e)
        {
            MaximizeWindow(sender, e);

            // Get the path of the image in the Debug folder
            string imagePath = System.IO.Path.Combine(Application.StartupPath, "intro.png");

            // Set the background image of the form
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }

        // TUIO listener methods
        public void addTuioObject(TuioObject o)
        {
            lock (objectList)
            {
                objectList.Add(o.SessionID, o);
            }

    
                if (o.SymbolID == 177)
                {
                this.Invoke((MethodInvoker)delegate {
                    button1_Click(null, EventArgs.Empty);
                });
                }
            if (o.SymbolID == 178)
            {
                this.Invoke((MethodInvoker)delegate {
                    button2_Click(null, EventArgs.Empty);
                });
            }



        }

        private void hlabel_Click(object sender, EventArgs e)
        {
            
        }
        private void StartTuioDemo(string exePath)
        {
            client.removeTuioListener(this);
            client.disconnect();
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide(); // Close the current form
            login newForm = new login();
            client.removeTuioListener(this);
            client.disconnect();
            newForm.ShowDialog(); // Open the new form
                                  //string exePath = Path.Combine(Application.StartupPath, @"..\..\..\..\Login\bin\Debug\Login.exe");
                                  //StartTuioDemo(exePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide(); // Close the current form
            Register newForm = new Register();
            client.removeTuioListener(this);
            client.disconnect();
            newForm.ShowDialog(); // Open the new form

            //string exePath = Path.Combine(Application.StartupPath, @"..\..\..\..\Login-Register Forms\Tarbita3.0\bin\Debug\Tarbita3.0.exe");
            //StartTuioDemo(exePath);
        }



        // Additional helper methods or event handlers can be added here if needed
    }
}
