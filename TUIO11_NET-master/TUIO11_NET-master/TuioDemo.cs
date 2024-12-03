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
using System.Linq;


/*
 
Kids Menu 
Add To Cart 
Reccomendation 
Quanitity in both main dishes and custom (Rotation)=> Haidy
Quantity of Toppings (Rotation) => Haidy

 */
public class MenuItem
{
    public string Name { get; }
    public string Category { get; }
    public bool IsGlutenFree { get; }
    public bool IsDiabeticFriendly { get; }
    public bool IsHealthy { get; }
    public decimal Price { get; }
    public string Description { get; }
    public string ImgPath { get; } // New property

    public MenuItem(string name, string category, bool isGlutenFree, bool isDiabeticFriendly, bool isHealthy, decimal price, string description, string imgPath)
    {
        Name = name;
        Category = category;
        IsGlutenFree = isGlutenFree;
        IsDiabeticFriendly = isDiabeticFriendly;
        IsHealthy = isHealthy;
        Price = price;
        Description = description;
        ImgPath = imgPath;

    }

    public override string ToString()
    {
        return $"{Name} ({Category}) - " +
               $"Gluten-Free: {(IsGlutenFree ? "Yes" : "No")}, " +
               $"Diabetic-Friendly: {(IsDiabeticFriendly ? "Yes" : "No")}, " +
               $"Healthy: {(IsHealthy ? "Yes" : "No")}, " +
               $"Price: ${Price:F2}, " +
               $"Description: {Description}, " +
               $"Image Path: {ImgPath}";
    }
}

public class Topping
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string ImgPath { get; set; }
    public int Quantity { get; set; }
}

public class CartItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public List<string> Toppings { get; set; } // For custom dishes like "Build Your Own Pizza"
}


public class TuioDemo : Form, TuioListener
{
    private TuioClient client;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;
    private Dictionary<int, List<Button>> objectButtons = new Dictionary<int, List<Button>>();

    public bool flagShift = true;
    public static int width, height;
    private int window_width = 1600;
    private int window_height = 950;
    private int window_left = 0;
    private int window_top = 0;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;


    public int page = 3;
    string category = "Lunch";

    public int ScreenMode;

    private bool IsGlutenFree = false;
    private bool IsDiabeticFriendly = false;
    private bool IsHealthy = true;
    private int Age = 22;
    private bool isRightHanded = true;

    List<MenuItem> menu = new List<MenuItem>
{
    // Breakfast items
    new MenuItem("Pancakes", "Breakfast", false, false, false, 5.99m, "Fluffy pancakes served with maple syrup.", "images/menu/breakfast/pancakes.png"),
    new MenuItem("Oatmeal", "Breakfast", true, true, true, 4.99m, "Healthy oatmeal topped with fresh fruits.", "images/menu/breakfast/oatmeal.png"),
    new MenuItem("Scrambled Eggs", "Breakfast", true, true, true, 3.99m, "Classic scrambled eggs with herbs.", "images/menu/breakfast/scrambled_eggs.png"),
    new MenuItem("Smoothie Bowl", "Breakfast", true, true, true, 6.99m, "Refreshing smoothie bowl with granola.", "images/menu/breakfast/smoothie_bowl.png"),
    new MenuItem("Avocado Toast", "Breakfast", false, true, true, 7.99m, "Toasted bread topped with mashed avocado.", "images/menu/breakfast/avocado_toast.png"),
    new MenuItem("Granola Bar", "Breakfast", true, true, true, 2.99m, "Crunchy granola bar with nuts.", "images/menu//breakfast/granola_bar.png"),
    new MenuItem("Bagel with Cream Cheese", "Breakfast", false, false, false, 3.49m, "Fresh bagel with a spread of cream cheese.", "images/menu/breakfast/bagel_cream_cheese.png"),
    new MenuItem("Fruit Salad", "Breakfast", true, true, true, 4.49m, "Assorted fresh fruit served chilled.", "images/menu/breakfast/fruit_salad.png"),
    new MenuItem("Yogurt Parfait", "Breakfast", true, true, true, 5.49m, "Layered yogurt with granola and berries.", "images/menu/breakfast/yogurt_parfait.png"),
    new MenuItem("Breakfast Burrito", "Breakfast", false, true, true, 8.49m, "Burrito filled with eggs, beans, and cheese.", "images/menu/breakfast/breakfast_burrito.png"),
    new MenuItem("French Toast", "Breakfast", false, false, false, 6.99m, "Classic French toast served with syrup.", "images/menu/breakfast/french_toast.png"),
    new MenuItem("Egg Muffin", "Breakfast", true, true, true, 3.99m, "Egg and cheese muffin for a quick bite.", "images/menu/breakfast/egg_muffin.png"),


    //Lunch
    new MenuItem("Grilled Chicken", "Lunch", false, true, true, 11.99m, "Juicy grilled chicken seasoned to perfection.", "images/menu/lunch/Grilled_Chicken.png"),
    new MenuItem("Tandori Pizza", "Lunch", false, false, true, 14.99m, "Spicy Tandori chicken pizza with a crispy crust.", "images/menu/lunch/Tandori_Pizza.png"),
    new MenuItem("Meat Lovers", "Lunch", false, false, false, 15.99m, "Loaded with assorted meats and cheese.", "images/menu/lunch/Meat_Lovers.png"),
    new MenuItem("Hot N Spicy Beef", "Lunch", false, false, false, 13.99m, "Beef pizza with a spicy kick.", "images/menu/lunch/Hot_N_Spicy_Beef.png"),
    new MenuItem("Vegetarian Pizza", "Lunch", true, true, true, 12.99m, "Loaded with fresh vegetables and cheese.", "images/menu/lunch/veg.png"),
    new MenuItem("Tuna Delight", "Lunch", true, true, true, 10.99m, "Tuna pizza with fresh toppings and cheese.", "images/menu/lunch/Tuna.png"),
    new MenuItem("Pepperoni Pizza", "Lunch", false, false, false, 13.49m, "Classic pepperoni pizza with a golden crust.", "images/menu/lunch/Pepperoni.png"),
    new MenuItem("Supreme Chicken", "Lunch", false, true, true, 14.99m, "Supreme chicken pizza with assorted toppings.", "images/menu/lunch/Supreme_Chicken.png"),
    new MenuItem("Seafood Pizza", "Lunch", true, true, true, 15.49m, "Fresh seafood pizza with premium ingredients.", "images/menu/lunch/Seafood.png"),
    new MenuItem("BBQ Chicken Pizza", "Lunch", false, true, true, 14.99m, "BBQ chicken pizza with smoky flavors.", "images/menu/lunch/BBQ_Chicken.png"),
    new MenuItem("Chicken Ranch Pizza", "Lunch", false, true, true, 13.99m, "Chicken ranch pizza with creamy ranch sauce.", "images/menu/lunch/Chicken_Ranch.png"),
    new MenuItem("Margarita Pizza", "Lunch", true, true, true, 11.99m, "Classic Margarita pizza with fresh basil.", "images/menu/lunch/Margarita.png"),
    

    // Dessert items
    new MenuItem("Cheesecake", "Dessert", false, false, false, 6.99m, "Rich and creamy cheesecake.", "images/menu/dessert/cheesecake.png"),
    new MenuItem("Cookies", "Dessert", false, false, false, 2.99m, "Freshly baked cookies.", "images/menu/dessert/cookies.png"),
    new MenuItem("Chocolate Cake", "Dessert", false, false, false, 6.99m, "Rich chocolate cake with frosting.", "images/menu/dessert/chocolate_cake.png"),
    new MenuItem("Cupcakes", "Dessert", false, false, false, 3.99m, "Decorated cupcakes with frosting.", "images/menu/dessert/cupcakes.png"),
    new MenuItem("Ice Cream", "Dessert", false, false, false, 3.99m, "Creamy ice cream in various flavors.", "images/menu/dessert/ice_cream.png"),
    new MenuItem("Pudding", "Dessert", true, false, false, 2.99m, "Smooth and creamy pudding.", "images/menu/dessert/pudding.png"),
    new MenuItem("Brownies", "Dessert", false, false, false, 5.49m, "Chocolate brownies with a fudgy center.", "images/menu/dessert/brownies.png"),
    new MenuItem("Apple Pie", "Dessert", false, false, false, 5.99m, "Traditional apple pie with spices.", "images/menu/dessert/apple_pie.png"),
    new MenuItem("Lemon Sorbet", "Dessert", true, true, true, 3.99m, "Refreshing lemon sorbet.", "images/menu/dessert/lemon_sorbet.png"),
    new MenuItem("Mousse", "Dessert", false, false, false, 4.49m, "Light and airy mousse.", "images/menu/dessert/mousse.png"),
    new MenuItem("Fruit Salad", "Dessert", true, true, true, 4.49m, "Fresh and healthy fruit salad.", "images/menu/dessert/fruit_salad.png"),
    new MenuItem("Frozen Yogurt", "Dessert", true, true, true, 4.99m, "Healthy frozen yogurt with toppings.", "images/menu/dessert/frozen_yogurt.png")
};


    List<Topping> toppings = new List<Topping>
{
    //new Topping { Name = "Mozzarella", Price = 10, ImgPath = "images/custom/Mozzarella.png", Quantity = 1 },
    new Topping { Name = "Peppers", Price = 5, ImgPath = "images/custom/Peppers.png", Quantity = 0 },
    new Topping { Name = "Tomatoes", Price = 5, ImgPath = "images/custom/Tomatoes.png", Quantity = 0 },
    new Topping { Name = "Onions", Price = 5, ImgPath = "images/custom/Onions.png", Quantity = 0 },
    new Topping { Name = "Olives", Price = 5, ImgPath = "images/custom/Olives.png", Quantity = 0 },
    new Topping { Name = "Mushrom", Price = 10, ImgPath = "images/custom/Mushrom.png", Quantity = 0 }
};


    // Menyu Page Scrolling
    public int currentMenuPage = 0;
    public float prevAngle = 0;
    public float currentAngle = 0;
    public bool isCurrentMenuPageChanged = false;


    private Process reactiVisionProcess; // Class-level variable to hold the process




    private bool fullscreen;
    private bool verbose;


    Font font = new Font("Arial", 10.0f);
    SolidBrush fntBrush = new SolidBrush(Color.White);
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
    SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
    SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
    SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);


    private JObject PerformCRUDOperation(string operation, object data)
    {
        string serverIp = "192.168.20.129";  // Replace with your server's IP address
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





    Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>> objectRectangles = new Dictionary<int, Tuple<Rectangle, Rectangle, Rectangle>>();
    int rotateIndex = 0;

    int getScreenMode()
    {
        TimeSpan currentTime = DateTime.Now.TimeOfDay;

        TimeSpan daylightStart = new TimeSpan(6, 0, 0);
        TimeSpan daylightEnd = new TimeSpan(18, 0, 0);

        // Determine the mode
        if (currentTime >= daylightStart && currentTime < daylightEnd)
        {
            return 0; // Daylight
        }
        else
        {
            return 1; // Nighttime
        }
    }




    // 0 => Rccomendation
    // 1=>Catrgories
    // 2 => Regular Menu
    // 3 => Build ur own
    // 5 => Checkout


    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        Graphics g = pevent.Graphics;
        ScreenMode = getScreenMode();
        switch (page)
        {
            case 0:

                break;
            case 1:

                break;
            case 2:
                drawPageCategories(g);
                break;
            case 3:
                drawPageMainMenu(g);
                break;
            case 4:
                drawPageCustom(g);
                break;
            default:
                break;
        }

    }

    void drawMainMenuBackground(Graphics g)
    {

        if (page == 2)
        {
            string path = "";
            if (ScreenMode == 0)
            {
                path = "light_categories.png";
            }
            else
            {
                path = "dark_categories.png";
            }

            using (Bitmap backgroundImage = (Bitmap)System.Drawing.Image.FromFile("images/" + path))
            {
                // Draw the bitmap to cover the entire window
                g.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height));
            }
        }
        else if (page == 3)
        {
            // el background image
            if (ScreenMode == 0)
            {

                string path = "";
                if (isRightHanded)
                {
                    path = "light_right.png";
                }
                else
                {
                    path = "light_left.png";
                }

                using (Bitmap backgroundImage = (Bitmap)System.Drawing.Image.FromFile("images/" + path))
                {
                    // Draw the bitmap to cover the entire window
                    g.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height));
                }
            }
            else
            {
                string path = "";

                if (isRightHanded)
                {
                    path = "dark_right.png";
                }
                else
                {
                    path = "dark_left.png";
                }
                using (Bitmap backgroundImage = (Bitmap)System.Drawing.Image.FromFile("images/" + path))
                {
                    // Draw the bitmap to cover the entire window
                    g.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height));
                }
            }
        }
        else if (page == 4)
        {
            // el background image
            if (ScreenMode == 0)
            {

                string path = "";
                if (isRightHanded)
                {
                    path = "light_custom_right.png";
                }
                else
                {
                    path = "light_custom_left.png";
                }

                using (Bitmap backgroundImage = (Bitmap)System.Drawing.Image.FromFile("images/" + path))
                {
                    // Draw the bitmap to cover the entire window
                    g.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height));
                }
            }
            else
            {
                string path = "";

                if (isRightHanded)
                {
                    path = "dark_custom_right.png";
                }
                else
                {
                    path = "dark_custom_left.png";
                }
                using (Bitmap backgroundImage = (Bitmap)System.Drawing.Image.FromFile("images/" + path))
                {
                    // Draw the bitmap to cover the entire window
                    g.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height));
                }
            }
        }

    }

    void drawMainMenuCircularRings(Graphics g, string category)
    {
        var selectedItems = menu
        .Where(item => item.Category == category)
        .Skip((currentMenuPage) * 4)
        .Take(4)
        .ToList();

        // el circular rings above plate
        int yStart = 40;
        int ringDiameter = 155;
        int spacing = 100;
        int totalWidth = (ringDiameter + spacing) * selectedItems.Count - spacing;
        int xStart = (this.Width - totalWidth) / 2;
        Brush brush;
        if (ScreenMode == 0)
        {
            brush = Brushes.Black;
        }
        else
        {
            brush = Brushes.White;
        }


        for (int i = 0; i < selectedItems.Count; i++)
        {
            var item = selectedItems[i];
            int x = xStart + i * (ringDiameter + spacing);
            int y = yStart;

            Color ringColor = (item.IsGlutenFree && item.IsDiabeticFriendly && item.IsHealthy) ? Color.Black : Color.Red;

            using (Pen pen = new Pen(ringColor, 5))
            {
                g.DrawEllipse(pen, x, y, ringDiameter, ringDiameter);
            }

            using (Font font = new Font("Arial", 12))
            {
                var textSize = g.MeasureString(item.Name, font);
                g.DrawString(item.Name, font, brush, x + (ringDiameter - textSize.Width) / 2, y + ringDiameter + 10);
            }
        }
    }


    void drawPageCategories(Graphics g)
    {
        drawMainMenuBackground(g);

        if (ScreenMode == 0)
        {
            page = 3;
            category = "Breakfast";
            return;
        }


        // Lunch or Dessert or Build ur Own
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
            objectCopy.AddRange(objectList.Values);
        }



        if (objectCopy.Count > 0)
        {

            lock (objectRectangles)
            {

                objectRectangles.Clear();

                foreach (TuioObject tobj in objectCopy)
                {
                    int ox = tobj.getScreenX(width);
                    int oy = tobj.getScreenY(height);
                    int size = height / 10;


                    float angleDegrees = (float)(tobj.Angle / Math.PI * 180.0f);


                    switch (tobj.SymbolID)
                    {
                        case 136:
                            category = "Lunch";
                            page = 3;
                            break;
                        case 137:
                            category = "Custom";
                            page = 4;
                            break;
                        case 138:
                            category = "Dessert";
                            page = 3;
                            break;

                        default:
                            break;
                    }

                }
            }



        }

    }

    void drawPageCustom(Graphics g)
    {
        List<bool> toppingAppears = new List<bool>(new bool[toppings.Count]);  // List for appearance flags

        drawMainMenuBackground(g);

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

        // Default Plate (when there no TUIO selected)
        string plate = Path.Combine(Environment.CurrentDirectory, "images/plate.png");
        using (System.Drawing.Image bgImage = System.Drawing.Image.FromFile(plate))
        {
            int bgwidth = bgImage.Width / 2 + 150;
            int bgheight = bgImage.Height / 2 + 150;
            g.DrawImage(bgImage, new Rectangle(new Point(this.Width / 2 - bgwidth / 2, this.Height / 2 - bgheight / 2), new Size(bgwidth, bgheight)));
        }

        string crust = Path.Combine(Environment.CurrentDirectory, "images/custom/pizza.png");
        using (System.Drawing.Image bgImage = System.Drawing.Image.FromFile(crust))
        {
            int bgwidth = bgImage.Width / 2 + 150;
            int bgheight = bgImage.Height / 2 + 150;
            g.DrawImage(bgImage, new Rectangle(new Point(this.Width / 2 - bgwidth / 2, this.Height / 2 - bgheight / 2), new Size(bgwidth, bgheight)));
        }

        string cheese = Path.Combine(Environment.CurrentDirectory, "images/custom/Mozzarella.png");
        using (System.Drawing.Image bgImage = System.Drawing.Image.FromFile(cheese))
        {
            int bgwidth = bgImage.Width / 2 + 150;
            int bgheight = bgImage.Height / 2 + 150;
            g.DrawImage(bgImage, new Rectangle(new Point(this.Width / 2 - bgwidth / 2, this.Height / 2 - bgheight / 2), new Size(bgwidth, bgheight)));
        }



        // draw the quantity of the toppings
        int spacing = 40;
        int yPosition = 755;
        int xPosition = 410;
        font = new Font("Arial", 18);
        Brush brush = Brushes.Black;
        foreach (var topping in toppings)
        {
            g.DrawString($"{topping.Quantity}", font, brush, new PointF(xPosition, yPosition));
            yPosition += spacing;
        }

        if (objectCopy.Count > 0)
        {
            lock (objectRectangles)
            {
                objectRectangles.Clear();

                TuioObject id15Object = objectCopy.Find(obj => obj.SymbolID == 15);

                if (id15Object != null)
                {
                    int id15X = id15Object.getScreenX(width);
                    int id15Y = id15Object.getScreenY(height);

                    int offset = 550;

                    // Initialize List of flags to track if each topping appears

                    foreach (TuioObject tobj in objectCopy)
                    {
                        if (tobj.SymbolID >= 16 && tobj.SymbolID < 21)
                        {
                            int objX = tobj.getScreenX(width);
                            int objY = tobj.getScreenY(height);

                            // Check if the object is within the offset range of id15Object
                            if (Math.Abs(objX - id15X) <= offset && Math.Abs(objY - id15Y) <= offset)
                            {
                                // Topping appears, set the flag for the corresponding topping
                                toppingAppears[tobj.SymbolID - 16] = true;

                                string toppingPath = toppings[tobj.SymbolID - 16].ImgPath;

                                using (System.Drawing.Image toppingImage = System.Drawing.Image.FromFile(toppingPath))
                                {
                                    int toppingWidth = toppingImage.Width / 2 + 150; // Adjust size
                                    int toppingHeight = toppingImage.Height / 2 + 150;

                                    g.DrawImage(toppingImage, new Rectangle(
                                        new Point(this.Width / 2 - toppingWidth / 2, this.Height / 2 - toppingHeight / 2),
                                        new Size(toppingWidth, toppingHeight)));
                                }
                            }
                        }


                        switch (tobj.SymbolID)
                        {

                            case 5:
                                page = 2;
                                break;
                            case 6:
                                page = 4;
                                break;
                            case 7:
                                // Add to cart

                                break;
                            default:
                                break;
                        }
                    }


                }
            }
        }

        for (int i = 0; i < toppingAppears.Count; i++)
        {
            if (toppingAppears[i])
            {
                toppings[i].Quantity = 1;
            }
            else
            {
                toppings[i].Quantity = 0;
            }
        }

    }

    bool canAddToCart = true;
    private List<CartItem> cart = new List<CartItem>();

    public void AddToCart(int removedItem, int currentMenuPage, List<string> selectedToppings = null, List<MenuItem> menuItems = null, int quantity = 1)
    {
        this.Text = removedItem + " ";
        if (!canAddToCart || removedItem == -1)
        {
            return;
        }



        int itemsPerPage = 4;
        int itemIndex = (currentMenuPage * itemsPerPage) + removedItem;



        var item = menuItems[itemIndex];
        var existingItem = cart.Find(itemm => itemm.Name == item.Name);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            return;
        }


        if (string.IsNullOrEmpty(item.Name) || item.Price <= 0 || quantity <= 0)
        {
            return;
        }


        var cartItem = new CartItem
        {
            Name = item.Name,
            Price = item.Price,
            Quantity = quantity,
            Toppings = selectedToppings
        };

        cart.Add(cartItem);

        canAddToCart = false;
    }

    public string GetCartAsString()
    {
        if (cart.Count == 0)
            return "Your cart is empty.";

        string cartDetails = "Cart Items:\n";
        foreach (var item in cart)
        {
            cartDetails += $"- {item.Name} (x{item.Quantity}) - ${item.Price * item.Quantity:F2}\n";

            if (item.Toppings != null && item.Toppings.Count > 0)
            {
                cartDetails += $"  Toppings: {string.Join(", ", item.Toppings)}\n";
            }
        }

        return cartDetails;
    }

    void drawPageMainMenu(Graphics g)
    {
        int quantity = 1;
        int isIDCart = -1;
        List<MenuItem> MenuItems;
        MenuItems = menu.Where(item => item.Category == category).ToList();

        drawMainMenuBackground(g);
        // retreive el menu
        drawMainMenuCircularRings(g, category);

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


        // Default Plate (when there no TUIO selected)
        string plate = Path.Combine(Environment.CurrentDirectory, "images/plate.png");
        using (System.Drawing.Image bgImage = System.Drawing.Image.FromFile(plate))
        {
            int bgwidth = bgImage.Width / 2 + 150; int bgheight = bgImage.Height / 2 + 150;
            g.DrawImage(bgImage, new Rectangle(new Point(this.Width / 2 - bgwidth / 2, this.Height / 2 - bgheight / 2 + 100), new Size(bgwidth, bgheight)));
        }

        if (objectCopy.Count > 0)
        {
            Dictionary<int, bool> itemRemovedFlags = new Dictionary<int, bool>
            {
                { 0, true },
                { 1, true },
                { 2, true },
                { 3, true }
            };

            int removedItem = -1;
            lock (objectRectangles)
            {

                objectRectangles.Clear(); // Clear previously stored rectangles

                foreach (TuioObject tobj in objectCopy)
                {
                    int ox = tobj.getScreenX(width);
                    int oy = tobj.getScreenY(height);
                    int size = height / 10;


                    float angleDegrees = (float)(tobj.Angle / Math.PI * 180.0f);


                    switch (tobj.SymbolID)
                    {
                        case 132:
                            itemRemovedFlags[0] = false;
                            break;
                        case 133:
                            itemRemovedFlags[1] = false;
                            break;
                        case 134:
                            itemRemovedFlags[2] = false;
                            break;
                        case 135:
                            itemRemovedFlags[3] = false;
                            break;
                        case 4:
                            currentMenuPage = changeCurrentMenuPage(angleDegrees); // Handle menu page change
                            break;
                        case 5:
                            page = 2;
                            break;
                        case 136:
                            //page = 4;
                            string cartDetails = GetCartAsString();
                            MessageBox.Show(cartDetails);
                            break;
                        case 137:
                            isIDCart = 1;
                            if (itemRemovedFlags.Values.Any(flag => flag))
                                removedItem = itemRemovedFlags.First(kv => kv.Value).Key;
                            AddToCart(removedItem, currentMenuPage, null, MenuItems, quantity);
                            break;
                        default:
                            break;
                    }

                }
            }

            if (isIDCart == -1)
            {
                canAddToCart = true;
            }

            // Add Kids Menu Option

            string name = "";
            string path = "";
            string description = "";
            decimal price;
            removedItem = -1;
            if (itemRemovedFlags.Values.Any(flag => flag))
                removedItem = itemRemovedFlags.First(kv => kv.Value).Key;



            MenuItems = menu.Where(item => item.Category == category).ToList();
            if (removedItem != -1)
            {
                int itemsPerPage = 4; //
                int index = (currentMenuPage * itemsPerPage) + removedItem;

                if (index >= 0 && index < MenuItems.Count())
                {
                    name = MenuItems[index].Name;
                    path = MenuItems[index].ImgPath;
                    description = MenuItems[index].Description;
                    price = MenuItems[index].Price;
                }
                else
                {
                    // Handle the case where the index is out of bounds
                    name = "";
                    path = "";
                    description = "Item not found";
                    price = 0M;
                }


                using (System.Drawing.Image bgImage = System.Drawing.Image.FromFile(path))
                {
                    int bgwidth = 500; int bgheight = 500;
                    g.DrawImage(bgImage, new Rectangle(new Point(this.Width / 2 - bgwidth / 2, this.Height / 2 - bgheight / 2 + 100), new Size(bgwidth, bgheight)));
                }

                this.Text = removedItem + " ," + currentMenuPage;


                string textToDraw = $"{"Price: " + price}\n\n{description}";

                RectangleF titledrawingArea = new RectangleF(60, 400, 1000, 400);
                RectangleF drawingArea = new RectangleF(60, 470, 450, 600);

                Font MainFont = new Font("Arial", 32);
                Font RegularFont = new Font("Arial", 18);
                Brush brush;
                if (ScreenMode == 0)
                {
                    brush = Brushes.Black;
                }
                else
                {
                    brush = Brushes.White;
                }


                g.DrawString(name, MainFont, brush, titledrawingArea);
                g.DrawString(textToDraw, RegularFont, brush, drawingArea);

            }




        }


    }


    bool isTUIOAppeared = false;

    int changeCurrentMenuPage(float angle)
    {
        if (!isTUIOAppeared)
        {

            prevAngle = angle;
            isTUIOAppeared = true;
            return 0;

        }

        float angleDifference = (angle - prevAngle);
        this.Text = "prev" + prevAngle + " " + "Curr" + angleDifference;


        if (angleDifference >= 40)
        {

            if (currentMenuPage < 2)
            {
                currentMenuPage += 1;
            }
            prevAngle = angle;
        }
        else if (angleDifference <= -40)
        {

            if (currentMenuPage > 0)
            {
                currentMenuPage -= 1;
            }
            prevAngle = angle;
        }


        return currentMenuPage;
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

            //    // Optionally set other properties like arguments
            reactiVisionProcess.StartInfo.Arguments = ""; // If you have any arguments

            //    // Start the process
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