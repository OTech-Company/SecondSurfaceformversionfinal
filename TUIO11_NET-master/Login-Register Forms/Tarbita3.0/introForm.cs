using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TUIO;

namespace Tarbita3._0
{
    public partial class introForm : Form, TuioListener
    {
        private TuioClient client;
        private Dictionary<long, TuioObject> objectList;

        public introForm()
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

    
                if (o.SymbolID == 0)
                {
                this.Invoke((MethodInvoker)delegate {
                    hlabel_Click(null, EventArgs.Empty);
                });
                }



            
        }

        private void hlabel_Click(object sender, EventArgs e)
        {
            this.Hide(); // Close the current form
            Register newForm = new Register();
            newForm.ShowDialog(); // Open the new form
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



        // Additional helper methods or event handlers can be added here if needed
    }
}
