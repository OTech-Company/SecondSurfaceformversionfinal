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
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using Timer = System.Windows.Forms.Timer;

public class TuioDemo : Form, TuioListener
{
    private TuioClient client;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;
    private Dictionary<int, List<Button>> objectButtons = new Dictionary<int, List<Button>>();

    public bool flagShift = true;
    public static int width, height;
    private int window_width = 900;
    private int window_height = 600;
    private int window_left = 0;
    private int window_top = 0;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;


    public event Action<int> PostsChanged; // Event for when posts change

    // Cache for posts
    private Dictionary<int, List<Post>> postCache = new Dictionary<int, List<Post>>();


    List<Post> posts = new List<Post>();
    private Timer holdTimer = new Timer();
    private int holdDuration = 1000; // 3 seconds in milliseconds
    private int previousRotateIndex = -1; // To track if rotateIndex has changed
    private bool inHoldRange = false;
    int postIndex = 0;

    private Process reactiVisionProcess; // Class-level variable to hold the process

    public class Post
    {
        public string CreatedAt { get; set; }
        public string Content { get; set; }
        public string PostId { get; set; }

        public override string ToString()
        {
            return $"Created At: {CreatedAt}\nContent: {Content}\nPost ID: {PostId}";
        }
    }


    private bool fullscreen;
    private bool verbose;

    private bool addTriggeredFlag = false, editTriggeredFlag = false;
    public List<string> Posts = new List<string>();

    Font font = new Font("Arial", 10.0f);
    SolidBrush fntBrush = new SolidBrush(Color.White);
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
    SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
    SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
    SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);


    List<string> CommentOptions = new List<string>
{
    "1. Exceeded Expectations",
    "2. Satisfactory",
    "3. Disappointing"
};
    private List<Bitmap> CircularMenu = new List<Bitmap>();
    int currentMenuFrame = 0;

    private JObject PerformCRUDOperation(string operation, object data)
    {
        string serverIp = "192.168.1.17";  // Replace with your server's IP address
        int serverPort = 9001;              // Replace with your server's port number
        try
        {
            // Create a TcpClient and connect to the server
            using (TcpClient client = new TcpClient())
            {
                client.Connect(serverIp, serverPort);

                // Prepare the JSON message for the CRUD operation
                var request = new
                {
                    operation = operation,
                    data = data
                };
                string jsonMessage = JsonConvert.SerializeObject(request);

                // Send the request to the server
                SendMessageToServer(client, jsonMessage);

                // Receive response from the server
                JObject response = ReceiveMessageFromServer(client);

                // Optionally show a message box with a specific field from the response

                // Close the client connection
                client.Close();

                return response; // Return the JSON response
            }
        }
        catch (Exception ex)
        {
            // Return an error JSON object with the exception message
            return new JObject { { "Error", "Error performing CRUD operation: " + ex.Message } };
        }
    }

    public void RefreshCache(int symbolID)
    {
        lock (postCache) // Locking to ensure thread safety
        {
            if (postCache.ContainsKey(symbolID))
            {
                // Refresh the cache for the specific SymbolID
                List<Post> updatedPosts = readTUIO(symbolID);
                postCache[symbolID] = updatedPosts;
            }
            else
            {
                // If it's a new SymbolID, cache the posts
                postCache[symbolID] = readTUIO(symbolID);
            }
        }
    }

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
            // Optionally log or handle the exception here
            //MessageBox.Show("Error sending message: " + ex.Message);
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

            // Validate if the response is a valid JSON
            if (IsValidJson(response))
            {
                return JObject.Parse(response); // Parse and return the JSON response
            }
            else
            {
                // Return an error JSON object
                return new JObject { { "Error", "Received data is not valid JSON." } };
            }
        }
        catch (Exception ex)
        {
            // Return an error JSON object with the exception message
            return new JObject { { "Error", "Error receiving message: " + ex.Message } };
        }
    }

    private bool IsValidJson(string response)
    {
        response = response.Trim();
        // Check if it starts with { or [ to identify JSON objects or arrays
        return (response.StartsWith("{") && response.EndsWith("}")) ||
               (response.StartsWith("[") && response.EndsWith("]"));
    }

    List<Post> readTUIO(int id)
    {
        // Get the JSON response from the server for the specified TUIO ID
        JObject response = PerformCRUDOperation("get_posts_by_tuio", new
        {
            tuio_id = id.ToString()  // Convert the id to a string
        });
        // List to store parsed posts
        List<Post> posts = new List<Post>();

        // Check if the response has a "posts" field containing an array
        if (response != null && response["posts"] is JArray postsArray)
        {

            for (int i = 0; i < postsArray.Count; i++)
            {
                // Create a new Post object and set its properties
                Post post = new Post
                {
                    CreatedAt = postsArray[i]["createdAt"]?.ToString() ?? "N/A",
                    Content = postsArray[i]["content"]?.ToString() ?? "N/A",
                    PostId = postsArray[i]["post_id"]?.ToString() ?? "N/A"
                };

                // Add the post to the list
                posts.Add(post);
            }
        }
        else
        {
            MessageBox.Show("Invalid response from server.");
        }

        // Return the list of posts
        return posts;
    }


    public void AddPost(int symbolID, Post newPost)
    {
        // Logic to add a new post...

        // After adding, trigger the event
        PostsChanged?.Invoke(symbolID); // Notify subscribers
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


        PostsChanged += RefreshCache; // Subscribe to the event


        this.Closing += new CancelEventHandler(Form_Closing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.DoubleBuffer, true);

        objectList = new Dictionary<long, TuioObject>(128);
        cursorList = new Dictionary<long, TuioCursor>(128);
        blobList = new Dictionary<long, TuioBlob>(128);

        this.Load += new System.EventHandler(this.TuioDemo_Load);
        holdTimer.Interval = holdDuration;
        holdTimer.Tick += HoldTimer_Tick;

        createCircularMenu();

        client = new TuioClient(port);
        client.addTuioListener(this);
        //InitializeComponent();
        client.connect();
    }

    private void HoldTimer_Tick(object sender, EventArgs e)
    {
        holdTimer.Stop();
        // rotate right 4
        //del 3
        // edit 2
        // rotate left 1
        // ADD 5

        switch (rotateIndex)
        {
            case 1:
                rotateLeftTriggered();
                addTriggeredFlag = false;
                editTriggeredFlag = false;
                postIndex = (postIndex - 1 + 4) % 4;
                break;
            case 2:
                editTriggered();
                addTriggeredFlag = false;
                editTriggeredFlag = true;
                break;
            case 3:
                deleteTriggered();
                addTriggeredFlag = false;
                editTriggeredFlag = false;
                break;
            case 4:
                rotateRightTriggered();
                addTriggeredFlag = false;
                editTriggeredFlag = false;
                postIndex = (postIndex + 1) % 4;
                break;
            case 5:
                addTriggeredFlag = true;
                editTriggeredFlag = false;
                break;
            default:
                break;
        }
    }

    private void rotateLeftTriggered()
    {
        this.Text = "Rotate Left";
    }

    private void editTriggered()
    {
        this.Text = "Edit";
    }

    private void deleteTriggered()
    {
        this.Text = "Delete";
    }

    private void rotateRightTriggered()
    {
        this.Text = "Rotate Right";

    }






    void createCircularMenu()
    {
        for (int i = 1; i <= 6; i++)
        {
            CircularMenu.Add(new Bitmap($"{i}.png")); // Adjust the path accordingly
        }
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

        if (reactiVisionProcess != null && !reactiVisionProcess.HasExited)
        {
            try
            {
                reactiVisionProcess.CloseMainWindow(); // Sends a close request to the main window
                reactiVisionProcess.WaitForExit(5000); // Wait for up to 5 seconds for the process to exit

                if (!reactiVisionProcess.HasExited)
                {
                    reactiVisionProcess.Kill(); // Force kill if it did not close gracefully
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to close reacTIVision: {ex.Message}");
            }
        }

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



    public static List<string> ReadPosts(int symbolID)
    {
        List<string> response;

        switch (symbolID)
        {
            case 4:
                response = new List<string>
                    {
        "Polo\n" +
        "'Best burger ever!'\n" +
        "'2024-10-20'",

        "Mark\n" +
        "'Fries were good, burger average.'\n" +
        "'2024-10-21'",

        "Heeey\n" +
        "'Great service, okay burger.'\n" +
        "'2024-10-22'"
    };
                break;

            case 1:
                response = new List<string>
                {
        "John Doe\n" +
        "'Graduation is here!'\n" +
        "'2024-10-23'",

        "Momen\n" +
        "'Excited for graduation!'\n" +
        "'2024-10-24'",

        "James\n" +
        "'Ready for grad day!'\n" +
        "'2024-10-25'"
    };
                break;

            case 2:
                response = new List<string>
                {
        "Hamooood\n" +
        "'Congrats, Mahmoud!'\n" +
        "'2024-10-26'",

        "Alios\n" +
        "'Well done, Mahmoud!'\n" +
        "'2024-10-27'",

        "Osamaaaz\n" +
        "'Great job, Mahmoud!'\n" +
        "'2024-10-28'"
    };
                break;

            case 3:
                response = new List<string>
                {
        "mazennashraff1\n" +
        "'Real Madrid wins again!'\n" +
        "'2024-10-29'",

        "Booooo\n" +
        "'Amazing game by Madrid!'\n" +
        "'2024-10-30'",

        "Mn3m\n" +
        "'Champions, Real Madrid!'\n" +
        "'2024-10-31'"
    };
                break;

            default:
                response = new List<string>
                {
                "No posts available for this symbol ID."
                };
                break;
        }

        return response;
    }

    Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>> objectRectangles = new Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>>();
    int rotateIndex = 0;



    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        Graphics g = pevent.Graphics;
        //g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

        using (Bitmap backgroundImage = (Bitmap)System.Drawing.Image.FromFile("BGG.png"))
        {
            // Draw the bitmap to cover the entire window
            pevent.Graphics.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height));
        }

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

                    string backgroundImagePath = "";


                    Posts = ReadPosts(tobj.SymbolID);
                    float angleDegrees = (float)(tobj.Angle / Math.PI * 180.0f);
                    bool menuFlag = false;


                    if (tobj.SymbolID == 0)
                    {
                        menuFlag = true;
                    }
                    else
                    {

                        if (!postCache.TryGetValue(tobj.SymbolID, out List<Post> posts))
                        {
                            // Posts not in cache, load them and notify subscribers
                            RefreshCache(tobj.SymbolID);
                            posts = postCache[tobj.SymbolID];
                            this.Text = "POSTSCACHE" + postCache.Count;// Retrieve from cache after refresh
                        }
                    }


                    switch (tobj.SymbolID)
                    {
                        case 0:
                            menuFlag = true;
                            break;
                        case 1:
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "MSA Graduation.jpg");

                            break;
                        case 2:
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "Graduation.jpg");

                            break;
                        case 3:
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "RealMadrid.jpg");

                            break;
                        case 4:
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "Willys.jpg");

                            break;
                        default:
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "Loading.jpg");

                            break;
                    }

                    try
                    {
                        // Draw the object rectangle
                        g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                        // Draw background image without rotation
                        if (File.Exists(backgroundImagePath))
                        {
                            using (System.Drawing.Image bgImage = System.Drawing.Image.FromFile(backgroundImagePath))
                            {
                                g.DrawImage(bgImage, new Rectangle(new Point(15, 10), new Size(530, 580)));
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Background image not found: {backgroundImagePath}");
                        }
                        // Draw the comments on the post and chnage it with rotation
                        // Get the angle in degrees from radians

                        if (menuFlag)
                        {
                            drawCircularMenu(g, angleDegrees);


                            if (addTriggeredFlag)
                            {


                                //Listen if gesture is DONE
                                string gestureRecognized = getGesture();


                                if (gestureRecognized == "one")
                                {

                                }
                                else if (gestureRecognized == "two")
                                {

                                }
                                else if (gestureRecognized == "three")
                                {

                                }
                                // Main prompt text
                                string commentText = "Select an option:";
                                Font font = new Font("Open Sans", 14, FontStyle.Regular);
                                Brush textBrush = Brushes.Black;
                                Brush backgroundBrush = new SolidBrush(Color.FromArgb(128, 255, 255, 255)); // White with 50% opacity

                                // Measure the size of the main prompt text
                                SizeF promptSize = g.MeasureString(commentText, font);
                                float rectanglePadding = 5; // Padding around the text

                                // Starting position for options
                                float startX = this.Width - 320;
                                float startY = this.Height - 150; // Starting Y position for the prompt
                                float lineSpacing = 32; // Spacing between each option line

                                // Measure the text width and height for background rectangle for options
                                SizeF optionTextSize = g.MeasureString(CommentOptions[0], font);

                                // Calculate total height of all options
                                float totalOptionsHeight = CommentOptions.Count * (optionTextSize.Height + lineSpacing) + promptSize.Height + 5; // Add prompt height and spacing

                                // Draw a semi-transparent background rectangle
                                RectangleF backgroundRect = new RectangleF(startX - rectanglePadding - 20, startY - rectanglePadding - 40, 310, totalOptionsHeight - 50);
                                g.FillRectangle(backgroundBrush, backgroundRect);

                                // Draw the main prompt text
                                g.DrawString(commentText, font, textBrush, startX, startY - 40);

                                // Loop through options and draw each one
                                for (int i = 0; i < CommentOptions.Count; i++)
                                {
                                    // Set position for each option
                                    PointF optionLocation = new PointF(startX, startY + (i * lineSpacing));

                                    g.DrawString(CommentOptions[i], font, textBrush, optionLocation);
                                }

                                // Dispose of the font object when done
                                font.Dispose();


                            }
                        }

                        if (tobj.SymbolID != 0)
                        {
                            // Define the position for the text
                            PointF textPosition1 = new PointF(ox, oy - size / 2);
                            PointF textPosition2 = new PointF(ox, oy - size + 10 / 2);

                            // Calculate the size of the rectangle based on text size
                            SizeF textSize1 = g.MeasureString(postCache[tobj.SymbolID][postIndex].CreatedAt, font);
                            SizeF textSize2 = g.MeasureString(postCache[tobj.SymbolID][postIndex].Content, font);

                            // Create the rectangle's position and size
                            RectangleF rect1 = new RectangleF(textPosition1, textSize1);
                            RectangleF rect2 = new RectangleF(textPosition2, textSize2);

                            // Draw the black rectangle behind the text
                            g.FillRectangle(Brushes.Black, rect1);
                            g.FillRectangle(Brushes.Black, rect2);

                            // Draw the text on top of the rectangle
                            g.DrawString(postCache[tobj.SymbolID][postIndex].CreatedAt, font, fntBrush, textPosition1);
                            g.DrawString(postCache[tobj.SymbolID][postIndex].Content, font, fntBrush, textPosition2);


                        }
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

                }
            }
        }

        // Similar logic for blobList: Create a copy and iterate
    }

    string getGesture()
    {

        return "";
    }


    void drawCircularMenu(Graphics g, float angleDegrees)
    {
        int imageWidth = CircularMenu[currentMenuFrame].Width - 250;
        int imgHeight = CircularMenu[currentMenuFrame].Height - 250;
        int x = this.Width - imageWidth;
        int y = this.Height - imgHeight;
        g.DrawImage(CircularMenu[rotateIndex], x - 60, 20, imageWidth, imgHeight);

        // Determine the rotateIndex based on angleDegrees
        if (angleDegrees >= 45.0f && angleDegrees < 125.0f)
        {
            SetRotateIndexWithHold(4);

            // rotate right 4
        }
        else if (angleDegrees >= 125.0f && angleDegrees < 180.0f)
        {
            SetRotateIndexWithHold(3);
            //del 3
        }
        else if (angleDegrees >= 180.0f && angleDegrees < 225.0f)
        {
            SetRotateIndexWithHold(2);
            // edit 2
        }
        else if (angleDegrees >= 225.0f && angleDegrees < 295.0f)
        {
            SetRotateIndexWithHold(1);
            // rotate left 1
        }
        else
        {
            SetRotateIndexWithHold(5);
            // ADD 5
        }

        this.Validate();
    }

    private void SetRotateIndexWithHold(int index)
    {
        if (rotateIndex != index)
        {
            // rotateIndex has changed, reset the timer and update the index
            rotateIndex = index;
            holdTimer.Stop();
            holdTimer.Start(); // Start the timer for the new rotateIndex
        }
        else if (!holdTimer.Enabled)
        {
            // Start the timer if it's not already running and rotateIndex hasn't changed
            holdTimer.Start();
        }

        previousRotateIndex = rotateIndex;
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
            Posts.Add(description);

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
        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\reacTIVision-1.5.1-win64\reacTIVision.exe");

        try
        {
            // Create a new process to start the executable
            reactiVisionProcess = new Process();
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
        System.Windows.Forms.Application.Run(app);
    }
}
