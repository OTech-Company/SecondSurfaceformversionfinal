using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MultiGamesPlatform
{
    public partial class Form1 : Form
    {
        private CartDisplay cartDisplay;
        private Panel cartPanel;

        public Form1()
        {
            InitializeComponent();

            try
            {
                string imagePath = @"C:\Users\Osama hosam\Source\Repos\OTech-Company\SecondSurfaceformversionfinal\TUIO11_NET-master\TUIO11_NET-master\bin\Debug\images\dark_checkout.png";

                // Check if the file exists
                if (File.Exists(imagePath))
                {
                    this.BackgroundImage = Image.FromFile(imagePath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;  // Resize the image to fit the form
                }
                else
                {
                    MessageBox.Show("Image file not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting background image: " + ex.Message);
            }
 
            // Initialize the Panel (cartPanel)
            cartPanel = new Panel
            {
                Location = new Point(70, 100),
                Size = new Size(1100, 700), // Adjust the size as needed
                AutoScroll = true, // Enable scrolling
                BackColor = Color.Transparent // Set background to transparent
            };

            // Add the Panel to the form's controls
            this.Controls.Add(cartPanel);

            // Sample cart items
            List<CartItem> cart = new List<CartItem>
    {
        new CartItem { Name = "Pizza", Price = 18, Quantity = 2, ImagePath = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\menu\\lunch\\Chicken_Pasta_Pizza.png" },
        new CartItem { Name = "Burger", Price = 12, Quantity = 3, ImagePath = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\menu\\breakfast\\french_toast.png"  },
        new CartItem { Name = "Pasta", Price = 15, Quantity = 1, ImagePath = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\menu\\dessert\\apple_pie.png"  },
                new CartItem { Name = "Pizza", Price = 18, Quantity = 2, ImagePath = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\menu\\lunch\\Chicken_Pasta_Pizza.png" },
        new CartItem { Name = "Burger", Price = 12, Quantity = 3, ImagePath = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\menu\\breakfast\\french_toast.png"  },
        new CartItem { Name = "Pasta", Price = 15, Quantity = 1, ImagePath = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\menu\\dessert\\apple_pie.png"  },

            };

            // Create the CartDisplay instance and pass the panel and cart items
            cartDisplay = new CartDisplay(cartPanel, cart);

            // Subscribe to the Paint event of the panel
            cartPanel.Paint += CartPanel_Paint;

            // Adjust the panel height for scrolling based on content
            cartDisplay.SetPanelHeight();
        }

        // Paint event handler for the cartPanel
        private void CartPanel_Paint(object sender, PaintEventArgs e)
        {
            // Call the DrawItems method to render the cart items
            cartDisplay.DrawItems(e.Graphics);
        }

        // Custom class to manage cart display and scrolling
        public class CartItem
        {
            public string Name { get; set; }

            public string ImagePath { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
        }

        // Custom class to handle cart display, drawing, and scrolling logic
        public class CartDisplay
        {
            private Panel cartPanel;
            private List<CartItem> cart;

            public CartDisplay(Panel cartPanel, List<CartItem> cart)
            {
                this.cartPanel = cartPanel;
                this.cart = cart;

                // Set initial panel properties
                this.cartPanel.AutoScroll = true;
            }

            // Method to set the panel height based on content
            public void SetPanelHeight()
            {
                // Define grid layout parameters
                int padding = 10;
                int rectHeight = 120;
                int numColumns = 1;  // Number of items per row

                // Calculate total height for the panel based on the number of items
                int totalHeight = (cart.Count / numColumns + (cart.Count % numColumns == 0 ? 0 : 1)) * (rectHeight + padding);

                // Explicitly set the AutoScrollMinSize to force the scrollable area
                cartPanel.AutoScrollMinSize = new Size(cartPanel.ClientSize.Width, totalHeight);
            }

            // Method to draw the cart items on the panel
            public void DrawItems(Graphics g)
            {
                // Define grid layout parameters for a single item per row
                int padding = 10;
                int rectWidth = 1000;  // Adjust width for the rectangle
                int rectHeight = 120;  // Adjust height for the rectangle
                int startX = 50;       // Fixed X position since we only display one item per row
                int startY = 0;        // Start Y position
                int numColumns = 1;    // Only one item per row, so columns = 1

                // Get the current scroll position
                int scrollOffset = cartPanel.VerticalScroll.Value;

                // Define reusable fonts and brushes
                Font titleFont = new Font("Arial", 14, FontStyle.Bold); // Bolder font for title
                Font detailsFont = new Font("Arial", 12, FontStyle.Regular); // Regular font for details
                Brush textBrush = Brushes.Black;
                Brush priceBrush = Brushes.Green; // Green color for price
                Pen borderPen = new Pen(Color.Black, 2);
                Brush backgroundBrush = new SolidBrush(Color.White); // Solid white background for each item

                // Loop through the cart items and draw them
                for (int i = 0; i < cart.Count; i++)
                {
                    // Calculate the Y position for the current item
                    int row = i;  // Since there's only 1 item per row, we use i directly for the row index

                    // Calculate the position of the rectangle, adjusted for scroll
                    float x = startX; // All items are aligned to the same X position
                    float y = startY + row * (rectHeight + padding) - scrollOffset;

                    // Ensure that y is within valid bounds (should not be negative)
                    if (y + rectHeight > cartPanel.ClientSize.Height) break;  // Stop drawing if we go past the bottom of the panel

                    // Draw the background color of the grid cell (solid color)
                    g.FillRectangle(backgroundBrush, x, y, rectWidth, rectHeight);

                    // Optionally: Draw the border for the grid cell (if you want the border)
                    g.DrawRectangle(borderPen, x, y, rectWidth, rectHeight);

                    // Format the text to display inside the rectangle
                    string itemImage = $"{cart[i].ImagePath}"; // Image path
                    string itemName = $"{cart[i].Name}";       // Item name
                    string itemPrice = $"{cart[i].Price}$";     // Item price
                    string itemQuantity = $"{cart[i].Quantity}"; // Item quantity
                    string addBtn = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\addBTN.png";
                    string minusBtn = "C:\\Users\\Osama hosam\\Source\\Repos\\OTech-Company\\SecondSurfaceformversionfinal\\TUIO11_NET-master\\TUIO11_NET-master\\bin\\Debug\\images\\minusBTN.png";

                    // Ensure the image is loaded correctly only if the path is valid
                    if (!string.IsNullOrEmpty(itemImage) && File.Exists(itemImage))
                    {
                        using (Bitmap img = (Bitmap)System.Drawing.Image.FromFile(itemImage))
                        {
                            // Set the size and position for the image (100x100 in this case)
                            Rectangle imgRect = new Rectangle((int)x + padding, (int)y + padding, 100, 100);
                            g.DrawImage(img, imgRect); // Draw the image at the specified location
                        }
                    }

                    // Draw the title in bold
                    g.DrawString(itemName, titleFont, textBrush, new PointF(x + 120 + padding, y + padding));

                    // Draw the price in green
                    g.DrawString(itemPrice, detailsFont, priceBrush, new PointF(x + 500 + padding, y + padding + 30));

                    // Draw Add button image (if valid)
                    if (!string.IsNullOrEmpty(addBtn) && File.Exists(addBtn))
                    {
                        using (Bitmap img = (Bitmap)System.Drawing.Image.FromFile(addBtn))
                        {
                            Rectangle imgRect = new Rectangle((int)x + 900 + padding, (int)y + padding, 50, 50);
                            g.DrawImage(img, imgRect);
                        }
                    }

                    // Draw Minus button image (if valid)
                    if (!string.IsNullOrEmpty(minusBtn) && File.Exists(minusBtn))
                    {
                        using (Bitmap img = (Bitmap)System.Drawing.Image.FromFile(minusBtn))
                        {
                            Rectangle imgRect = new Rectangle((int)x + 900 - 120 + padding, (int)y + padding, 50, 50);
                            g.DrawImage(img, imgRect);
                        }
                    }

                    // Draw the quantity in black
                    g.DrawString(itemQuantity, detailsFont, textBrush, new PointF(x + 860 + padding, y + padding + 30));
                }

  
                // Dispose of reusable resources after the loop
                titleFont.Dispose();
                detailsFont.Dispose();
                borderPen.Dispose();
                backgroundBrush.Dispose(); // Dispose of the background brush
            }



        }
    }
}
