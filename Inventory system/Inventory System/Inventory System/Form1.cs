using System;
using System.Windows.Forms;

namespace Inventory_System
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database notice: " + ex.Message, "Database Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #region Authentication
        private void btnSignIn_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter your username.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                string fullName;
                bool success = DatabaseHelper.AuthenticateUser(username, password, out fullName);

                if (success)
                {
                    MessageBox.Show("Welcome, " + fullName + "!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtPassword.Clear();

                    DashboardForm dashboard = new DashboardForm(fullName, this);
                    this.Hide();
                    dashboard.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Navigation
        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            RegisterForm regForm = new RegisterForm(this);
            this.Hide();
            regForm.Show();
        }

        public void SetRegisteredUser(string username)
        {
            txtUsername.Text = username;
            txtPassword.Clear();
            txtPassword.Focus();
        }
        #endregion

        #region UI Controls
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '●';
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
        }

        private void lblPassword_Click(object sender, EventArgs e)
        {
        }

        private void lblNoAccount_Click(object sender, EventArgs e)
        {
        }
        #endregion

        private void lblBrandTitle_Click(object sender, EventArgs e)
        {

        }

        private void lblNoAccount_Click_1(object sender, EventArgs e)
        {

        }
    }
}
