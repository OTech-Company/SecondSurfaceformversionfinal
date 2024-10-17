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
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
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


    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        Graphics g = pevent.Graphics;
         g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

        // Draw the cursor path
        if (cursorList.Count > 0)
        {
            lock (cursorList)
            {
                foreach (TuioCursor tcur in cursorList.Values)
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
        }

        // Draw the objects
        if (objectList.Count > 0)
        {
            lock (objectList)
            {
                // Clear existing buttons before drawing new ones

                foreach (TuioObject tobj in objectList.Values)
                {
                    int ox = tobj.getScreenX(width);
                    int oy = tobj.getScreenY(height);
                    int size = height / 10;


                    // Load object image based on SymbolID
                  
                    try
                    {
                        // Draw object image with rotation
                       // Retrieve description using the function objDescriptions based on SymbolID
                        string[] descriptions = objDescriptions(tobj.SymbolID);

                        // Loop through each string in the description array and display each in a new row
                        for (int i = 0; i < descriptions.Length; i++)
                        {
                            // Adjust Y position for each string to be displayed in the next row
                            g.DrawString(descriptions[i], new Font("Arial", 12), Brushes.Black, ox + size / 2 + 10, oy + (i * 20));
                        }
                       
                        g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                       
                    
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error drawing object: {ex.Message}");
                    }

                   CreateButtons(tobj.SymbolID);

                }

                // Create buttons at the bottom of the form
            }
        }
        else
        {
            ClearButtons();
        }

        // Draw the blobs
        if (blobList.Count > 0)
        {
            lock (blobList)
            {
                foreach (TuioBlob tblb in blobList.Values)
                {
                    int bx = tblb.getScreenX(width);
                    int by = tblb.getScreenY(height);
                    float bw = tblb.Width * width;
                    float bh = tblb.Height * height;

                    g.TranslateTransform(bx, by);
                    g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
                    g.TranslateTransform(-bx, -by);

                    g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

                    g.TranslateTransform(bx, by);
                    g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
                    g.TranslateTransform(-bx, -by);

                    g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
                }
            }
        }
    }

    private void CreateButtons(int id)
    {


        // Clear existing buttons first
        // ClearButtons();

        int buttonWidth = this.ClientSize.Width / 3; // Full width divided by 3
        int buttonHeight = 40; // Set a height for the buttons

        Button addButton = new Button
        {
            Text = "Add",
            Location = new Point(0, this.ClientSize.Height - buttonHeight), // Bottom left
            Size = new Size(buttonWidth, buttonHeight)
        };
        addButton.Click += (sender, e) => AddTuioObject(id);
        addButton.BringToFront();

        this.Controls.Add(addButton); // Add button to the form

        Button updateButton = new Button
        {
            Text = "Update",
            Location = new Point(buttonWidth, this.ClientSize.Height - buttonHeight), // Bottom middle
            Size = new Size(buttonWidth, buttonHeight)
        };
        updateButton.BringToFront();

        updateButton.Click += (sender, e) => UpdateTuioObject(id);
        this.Controls.Add(updateButton); // Add button to the form

        Button deleteButton = new Button
        {
            Text = "Delete",
            Location = new Point(buttonWidth * 2, this.ClientSize.Height - buttonHeight), // Bottom right
            Size = new Size(buttonWidth, buttonHeight)
        };
        deleteButton.Click += (sender, e) => DeleteTuioObject(id);

        // Bring buttons to the front
        deleteButton.BringToFront();

        this.Controls.Add(deleteButton); // Add button to the form

    }

    private void ClearButtons()
    {
        // Clear existing buttons from the form
        foreach (Control control in this.Controls)
        {
            if (control is Button && control.Text == "Add" || control.Text == "Update" || control.Text == "Delete")
            {
                this.Controls.Remove(control);
                control.Dispose(); // Optional: Dispose of the button to free resources
            }
        }

        this.Invalidate();

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
