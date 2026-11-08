using System;
using System.Windows.Forms;

namespace Inventory_System
{
    public partial class DashboardForm : Form
    {
        private string _fullName;
        private Form1 _loginForm;
        private bool _isLoggingOut = false;

        public DashboardForm(string fullName, Form1 loginForm)
        {
            InitializeComponent();
            _fullName = fullName;
            _loginForm = loginForm;

            lblWelcome.Text = "Welcome";
            lblUser.Text = fullName;
        }

        #region Dashboard Actions
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Log out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _isLoggingOut = true;
                this.Close();
                _loginForm.Show();
            }
        }

        private void DashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isLoggingOut && e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }

        private void lblWelcome_Click(object sender, EventArgs e)
        {
        }

        private void lblUser_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
        #endregion

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
