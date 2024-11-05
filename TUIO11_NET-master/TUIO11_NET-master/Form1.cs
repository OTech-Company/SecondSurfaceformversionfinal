using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
// some edits 

/*
fix01(lackOfPackage)
    If you encounter the error: 
    "The type name 'OleDbConnection' could not be found in the namespace 'System.Data.OleDb'",
    follow these steps to resolve it:

    1. Open Visual Studio.
    2. In Solution Explorer, right-click on your project name (e.g., TheSocialNetwork_AR_Login_Registration).
    3. Select "Manage NuGet Packages".
    4. In the NuGet window, go to the "Browse" tab.
    5. Search for "System.Data.OleDb" and install it.
    6. Rebuild your project.
 read comments save yourself some time ladz -_-
*/

namespace TheSocialNetwork_AR_Login_Registration
{
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();
            
           
        }
        //Database linking code
        //OleDbConnection con = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=db_users.mdb");
        //OleDbCommand cmd = new OleDbCommand();
        //OleDbDataAdapter da = new OleDbDataAdapter();
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmRegister_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Validate input
            if (txtUsername.Text == "" && txtPassword.Text =="" && TxtComPassword.Text == "")
            {
                MessageBox.Show("Username and Password fields are empty", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (txtPassword.Text == TxtComPassword.Text)
            {
                //con.Open();
                //string register = "INSERT INTO tbl_users VALUES ('" + txtUsername.Text + "','" + txtPassword.Text + "');";
                //cmd = new OleDbCommand(register, con);
                //cmd.ExecuteNonQuery(); 
                //con.Close();

                txtUsername.Text = "";
                txtPassword.Text = "";
                TxtComPassword.Text = "";

                MessageBox.Show("Your Account has been successfully Created", "Registration Success",MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("Passwords does not match, please Re-enter","Registration Failed",MessageBoxButtons.OK,MessageBoxIcon.Error);
                txtPassword.Text = "";
                TxtComPassword.Text = "";
                txtPassword.Focus();

            }

        }

       

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0';
                TxtComPassword.PasswordChar = '\0';
            }    
            else
            {
                txtPassword.PasswordChar = '•'; // Mask with bullet points
                TxtComPassword.PasswordChar = '•'; // Mask with bullet points

            }
        }

        private void txtComPassword_Click(object sender, EventArgs e)
        {

        }

        private void TxtcomPas_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            txtUsername.Text = "";
            txtPassword.Text = "";
            TxtComPassword.Text = "";
            txtUsername.Focus();
        }
    }
}
