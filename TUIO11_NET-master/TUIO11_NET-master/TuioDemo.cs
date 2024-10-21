/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;


public class TuioDemo : Form, TuioListener
{
    private TuioClient client;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;
    private Dictionary<int, List<Button>> objectButtons = new Dictionary<int, List<Button>>();


    public static int width, height;
    private int window_width = 640;
    private int window_height = 480;
    private int window_left = 0;
    private int window_top = 0;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;

    private bool fullscreen;
    private bool verbose;

    Font font = new Font("Arial", 10.0f);
    SolidBrush fntBrush = new SolidBrush(Color.White);
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
    SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
    SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
    SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
    
    
    
    // Define the relative path to access Solution_items/server.py
    string clientfile = @"..\Solution_items\server.py";

    public void ReadMyFiles()
    {
        if (System.IO.File.Exists(clientfile))
        {
            string fileContents = System.IO.File.ReadAllText(clientfile);
            Console.WriteLine(fileContents);
        }
        else
        {
            Console.WriteLine("File not found.");
        }
    }

    public TuioDemo(int port)
    {

        verbose = false;
        fullscreen = false;
        width = window_width;
        height = window_height;

        this.ClientSize = new System.Drawing.Size(width, height);
        this.Name = "TuioDemo";
        this.Text = "TuioDemo";
        this.DoubleBuffered = true; // Enable double buffering

        this.Closing += new CancelEventHandler(Form_Closing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.DoubleBuffer, true);

        objectList = new Dictionary<long, TuioObject>(128);
        cursorList = new Dictionary<long, TuioCursor>(128);
        blobList = new Dictionary<long, TuioBlob>(128);

        this.Load += new System.EventHandler(this.TuioDemo_Load);

        client = new TuioClient(port);
        client.addTuioListener(this);
        //InitializeComponent();
        client.connect();
    }

    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {

        if (e.KeyData == Keys.F1)
        {
            if (fullscreen == false)
            {

                width = screen_width;
                height = screen_height;

                window_left = this.Left;
                window_top = this.Top;

                this.FormBorderStyle = FormBorderStyle.None;
                this.Left = 0;
                this.Top = 0;
                this.Width = screen_width;
                this.Height = screen_height;

                fullscreen = true;
            }
            else
            {

                width = window_width;
                height = window_height;

                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Left = window_left;
                this.Top = window_top;
                this.Width = window_width;
                this.Height = window_height;

                fullscreen = false;
            }
        }
        else if (e.KeyData == Keys.Escape)
        {
            this.Close();

        }
        else if (e.KeyData == Keys.V)
        {
            verbose = !verbose;
        }

    }

    private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        client.removeTuioListener(this);

        client.disconnect();
        System.Environment.Exit(0);
    }

    public void addTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Add(o.SessionID, o);
        }
        if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
    }

    public void updateTuioObject(TuioObject o)
    {

        if (verbose) Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);

    }

    public void removeTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Remove(o.SessionID);
        }
        if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");

    }

    public void addTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Add(c.SessionID, c);
        }
        if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
    }

    public void updateTuioCursor(TuioCursor c)
    {
        if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);

    }

    public void removeTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Remove(c.SessionID);
        }
        if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");

    }

    public void addTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Add(b.SessionID, b);
        }
        if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);

    }

    public void updateTuioBlob(TuioBlob b)
    {

        if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);

    }

    public void removeTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Remove(b.SessionID);
        }
        if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");

    }

    public void refresh(TuioTime frameTime)
    {
        Invalidate();
    }

    string[] objDescriptions(int symbolID)
    {
        switch (symbolID)
        {
            case 1:
                return new string[] { "This is a circle", "It is round", "It has no corners" };
            case 2:
                return new string[] { "This is a square", "It has 4 sides", "Each side is equal" };
            case 3:
                return new string[] { "This is a triangle", "It has 3 sides", "It has 3 angles" };
            default:
                return new string[] { "Unknown object" };
        }
    }

    Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>> objectRectangles = new Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>>();

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        Graphics g = pevent.Graphics;
        g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

        // Copy of cursorList to safely iterate
        List<TuioCursor> cursorCopy = new List<TuioCursor>();

        lock (cursorList)
        {
            cursorCopy.AddRange(cursorList.Values); // Make a safe copy of cursorList
        }

        // Draw the cursor path
        if (cursorCopy.Count > 0)
        {
            foreach (TuioCursor tcur in cursorCopy)
            {
                List<TuioPoint> path = tcur.Path;
                TuioPoint current_point = path[0];

                for (int i = 0; i < path.Count; i++)
                {
                    TuioPoint next_point = path[i];
                    g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
                    current_point = next_point;
                }
                g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
                g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
            }
        }

        // Copy of objectList to safely iterate
        List<TuioObject> objectCopy = new List<TuioObject>();

        lock (objectList)
        {
            objectCopy.AddRange(objectList.Values); // Make a safe copy of objectList
        }

        // Draw the objects
        if (objectCopy.Count > 0)
        {
            lock (objectRectangles)
            {
                objectRectangles.Clear(); // Clear previously stored rectangles

                foreach (TuioObject tobj in objectCopy)
                {
                    int ox = tobj.getScreenX(width);
                    int oy = tobj.getScreenY(height);
                    int size = height / 10;

                    try
                    {
                        // Draw object image and descriptions
                        string[] descriptions = objDescriptions(tobj.SymbolID);
                        for (int i = 0; i < descriptions.Length; i++)
                        {
                            g.DrawString(descriptions[i], new Font("Arial", 12), Brushes.Black, ox + size / 2 + 10, oy + (i * 20));
                        }

                        // Draw the object rectangle
                        g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error drawing object: {ex.Message}");
                    }

                    int rectWidth = size * 2; // Increase the width (you can adjust the multiplier as needed)
                    int rectHeight = 30; // Fixed height for the rectangles

                    // Calculate the positions for the Add, Update, and Delete rectangles with no space between them
                    Rectangle addRect = new Rectangle(ox - rectWidth / 2, oy + size, rectWidth, rectHeight);
                    Rectangle updateRect = new Rectangle(ox - rectWidth / 2, oy + size + rectHeight, rectWidth, rectHeight); // Directly under Add
                    Rectangle deleteRect = new Rectangle(ox - rectWidth / 2, oy + size + rectHeight * 2, rectWidth, rectHeight); // Directly under Update


                    // Store the rectangles in the dictionary with the SymbolID as key
                    objectRectangles[tobj.SymbolID] = new Tuple<Rectangle, Rectangle, Rectangle>(addRect, updateRect, deleteRect);
                    objectRectangles[tobj.SymbolID] = new Tuple<Rectangle, Rectangle, Rectangle>(addRect, updateRect, deleteRect);


                    // Draw rectangles
                    g.FillRectangle(Brushes.Green, addRect);
                    g.DrawString("Add", new Font("Arial", 12), Brushes.White, addRect.X + 5, addRect.Y + 5);

                    g.FillRectangle(Brushes.Orange, updateRect);
                    g.DrawString("Update", new Font("Arial", 12), Brushes.White, updateRect.X + 5, updateRect.Y + 5);

                    g.FillRectangle(Brushes.Red, deleteRect);
                    g.DrawString("Delete", new Font("Arial", 12), Brushes.White, deleteRect.X + 5, deleteRect.Y + 5);
                }
            }
        }

        // Similar logic for blobList: Create a copy and iterate
    }

    // Mouse click event handler to check if a rectangle is clicked
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        // Copy objectRectangles to safely iterate during mouse click event
        Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>> rectanglesCopy = new Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>>(objectRectangles);

        foreach (var kvp in rectanglesCopy)
        {
            int symbolID = kvp.Key;
            var rects = kvp.Value;

            // Check if the click is within the "Add" rectangle
            if (rects.Item1.Contains(e.Location))
            {
                addButton(symbolID);
            }
            // Check if the click is within the "Update" rectangle
            else if (rects.Item2.Contains(e.Location))
            {
                updateButton(symbolID);
            }
            // Check if the click is within the "Delete" rectangle
            else if (rects.Item3.Contains(e.Location))
            {
                deleteButton(symbolID);
            }
        }
    }

    private void addButton(int id)
    {
        // Create a panel
        int width = this.Width;
        Panel panel = new Panel();
        panel.Size = new Size(width, 80); // Set the size of the panel
        panel.BorderStyle = BorderStyle.FixedSingle; // Adds a border to the panel
        panel.Location = new Point(0, this.ClientSize.Height - panel.Height); // Position at bottom of form
        panel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; // Stick it to the bottom

        // Create a label
        Label lblDescription = new Label();
        lblDescription.Text = "Enter Post Description:";
        lblDescription.Location = new Point(10, 10);
        lblDescription.AutoSize = true;

        // Create a text box with increased height
        TextBox txtDescription = new TextBox();
        txtDescription.Location = new Point(10, 40);
        txtDescription.Width = 200; // Adjust width for text box
        txtDescription.Height = 30; // Set the height to match the button

        // Create a submit button with the same height as the text box
        Button btnSubmit = new Button();
        btnSubmit.Text = "Submit";
        btnSubmit.Location = new Point(220, 40); // Position it next to the text box
        btnSubmit.Width = 80; // Set a suitable width
        btnSubmit.Height = txtDescription.Height; // Match the height of the text box

        // Button click event to capture the description and call addRequest
        btnSubmit.Click += (sender, e) =>
        {
            // Capture the description
            string description = txtDescription.Text;

            // Call the addRequest function with id and description
            addRequest(id, description);

            // Remove the panel from the form (and all controls inside it)
            this.Controls.Remove(panel);
        };

        // Add controls to the panel
        panel.Controls.Add(lblDescription);
        panel.Controls.Add(txtDescription);
        panel.Controls.Add(btnSubmit);

        // Add the panel to the form
        this.Controls.Add(panel);
    }

    // Define the addRequest function
    private void addRequest(int id, string description)
    {
        // Implement your logic to handle the request here
        MessageBox.Show($"Request added for object {id}: {description}");
        // Add additional logic to save the description, update a database, etc.
    }




    private void updateButton(int id)
    {
        // Create a panel
        int width = this.Width;
        Panel panel = new Panel();
        panel.Size = new Size(width, 80); // Set the size of the panel
        panel.BorderStyle = BorderStyle.FixedSingle; // Adds a border to the panel
        panel.Location = new Point(0, this.ClientSize.Height - panel.Height); // Position at bottom of form
        panel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; // Stick it to the bottom

        // Create a label
        Label lblDescription = new Label();
        lblDescription.Text = "Please enter updated data below:";
        lblDescription.Location = new Point(10, 10);
        lblDescription.AutoSize = true;

        // Create a text box with increased height
        TextBox txtDescription = new TextBox();
        txtDescription.Location = new Point(10, 40);
        txtDescription.Width = 200; // Adjust width for text box
        txtDescription.Height = 30; // Set the height to match the button

        // Create a submit button with the same height as the text box
        Button btnSubmit = new Button();
        btnSubmit.Text = "Submit";
        btnSubmit.Location = new Point(220, 40); // Position it next to the text box
        btnSubmit.Width = 80; // Set a suitable width
        btnSubmit.Height = txtDescription.Height; // Match the height of the text box

        // Button click event to capture the description and call addRequest
        btnSubmit.Click += (sender, e) =>
        {
            // Capture the description
            string description = txtDescription.Text;

            // Call the addRequest function with id and description
            updateRequest(id, description);

            // Remove the panel from the form (and all controls inside it)
            this.Controls.Remove(panel);
        };

        // Add controls to the panel
        panel.Controls.Add(lblDescription);
        panel.Controls.Add(txtDescription);
        panel.Controls.Add(btnSubmit);

        // Add the panel to the form
        this.Controls.Add(panel);
    }

    // Define the addRequest function
    private void updateRequest(int id, string description)
    {
        // Implement your logic to handle the request here
        MessageBox.Show($"Update request for object {id}: {description}");
        // Add additional logic to save the description, update a database, etc.
    }



    private void deleteButton(int id)
    {
        // Create a panel
        int width = this.Width;
        Panel panel = new Panel();
        panel.Size = new Size(width, 80); // Set the size of the panel
        panel.BorderStyle = BorderStyle.FixedSingle; // Adds a border to the panel
        panel.Location = new Point(0, this.ClientSize.Height - panel.Height); // Position at bottom of form
        panel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; // Stick it to the bottom

        // Create a label for confirmation
        Label lblConfirmation = new Label();
        lblConfirmation.Text = "Are you sure you want to delete?";
        lblConfirmation.Location = new Point(10, 10);
        lblConfirmation.AutoSize = true;

        // Create a "Yes" button
        Button btnYes = new Button();
        btnYes.Text = "Yes";
        btnYes.Location = new Point(10, 40); // Position it
        btnYes.Width = 80; // Set a suitable width

        // Button click event to call deleteRequest
        btnYes.Click += (sender, e) =>
        {
            // Call the deleteRequest function with id
            deleteRequest(id);

            // Remove the panel from the form (and all controls inside it)
            this.Controls.Remove(panel);
        };

        // Create a "No" button
        Button btnNo = new Button();
        btnNo.Text = "No";
        btnNo.Location = new Point(100, 40); // Position it next to the "Yes" button
        btnNo.Width = 80; // Set a suitable width

        // Button click event to remove the panel without action
        btnNo.Click += (sender, e) =>
        {
            // Remove the panel from the form (and all controls inside it)
            this.Controls.Remove(panel);
        };

        // Add controls to the panel
        panel.Controls.Add(lblConfirmation);
        panel.Controls.Add(btnYes);
        panel.Controls.Add(btnNo);

        // Add the panel to the form
        this.Controls.Add(panel);
    }

    // Define the deleteRequest function
    private void deleteRequest(int id)
    {
        // Implement your logic to handle the delete request here
        MessageBox.Show($"Delete request for object {id} confirmed.");
        // Add additional logic to delete the object from the database, etc.
    }




    // mas2ola 3n shakl el form el odamy


    // Example event handlers
    void AddTuioObject(int id)
    {
        MessageBox.Show("Add" + id);
    }

    void UpdateTuioObject(int id)
    {
        MessageBox.Show("Update" + id);
    }

    void DeleteTuioObject(int id)
    {
        MessageBox.Show("Delete" + id);
    }

    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // TuioDemo
            // 
            this.ClientSize = new System.Drawing.Size(647, 479);
            this.Name = "TuioDemo";
            this.Load += new System.EventHandler(this.TuioDemo_Load);
            this.ResumeLayout(false);

    }

    private void TuioDemo_Load(object sender, EventArgs e)
    {
        // Path to the reacTIVision executable
        // Get the current directory of the executable (project directory)
        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\reacTIVision-1.5.1-win64\reacTIVision.exe");

        try
        {
            // Create a new process to start the executable
            Process reactiVisionProcess = new Process();
            reactiVisionProcess.StartInfo.FileName = exePath;

            // Optionally set other properties like arguments
            reactiVisionProcess.StartInfo.Arguments = ""; // If you have any arguments

            // Start the process
            reactiVisionProcess.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start reacTIVision: {ex.Message}");
        }
    }

    public static void Main(String[] argv)
    {
        int port = 0;
        switch (argv.Length)
        {
            case 1:
                port = int.Parse(argv[0], null);
                if (port == 0) goto default;
                break;
            case 0:
                port = 3333;
                break;
            default:
                Console.WriteLine("usage: mono TuioDemo [port]");
                System.Environment.Exit(0);
                break;
        }

        TuioDemo app = new TuioDemo(port);
        Application.Run(app);
    }
}
