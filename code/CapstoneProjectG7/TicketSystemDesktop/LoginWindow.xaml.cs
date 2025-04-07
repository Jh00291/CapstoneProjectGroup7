using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using TicketSystemDesktop.Data;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                LoginStatus.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                using (var context = new TicketDBContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserName == username);

                    if (user == null)
                    {
                        LoginStatus.Text = "User not found.";
                        return;
                    }

                    var hasher = new PasswordHasher<Employee>();
                    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        var main = new MainWindow(user);
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        LoginStatus.Text = "Invalid password.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }
    }
}
