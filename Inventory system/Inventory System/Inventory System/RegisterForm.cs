using System;
using System.Windows.Forms;

namespace Inventory_System
{
    public partial class RegisterForm : Form
    {
        private Form1 _loginForm;

        public RegisterForm(Form1 loginForm)
        {
            InitializeComponent();
            _loginForm = loginForm;
        }

        #region Registration
        private void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Please enter your full name.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

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

            if (password.Length < 4)
            {
                MessageBox.Show("Password must be at least 4 characters long.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please re-enter your password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Clear();
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                string errorMessage;
                bool success = DatabaseHelper.RegisterUser(fullName, username, password, out errorMessage);

                if (success)
                {
                    MessageBox.Show(
                        "Welcome, " + fullName + "!\nRegistered successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    _loginForm.SetRegisteredUser(username);
                    this.Hide();
                    _loginForm.Show();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUsername.SelectAll();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Navigation
        private void btnBackToLogin_Click(object sender, EventArgs e)
        {
            this.Hide();
            _loginForm.Show();
        }

        private void RegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                _loginForm.Show();
            }
        }
        #endregion

        #region UI Controls
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            char pChar = chkShowPassword.Checked ? '\0' : '●';
            txtPassword.PasswordChar = pChar;
            txtConfirmPassword.PasswordChar = pChar;
        }
        #endregion

        private void lblBrandSubtitle_Click(object sender, EventArgs e)
        {

        }
    }
}
