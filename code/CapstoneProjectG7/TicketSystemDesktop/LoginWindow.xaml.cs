using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using TicketSystemDesktop.Data;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop
{
    /// <summary>
    /// login window
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the Login control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
                    var hasher = new PasswordHasher<Employee>();

                    if (user != null)
                    {
                        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
                        if (result == PasswordVerificationResult.Success)
                        {
                            var main = new MainWindow(user);
                            main.Show();
                            this.Close();
                            return;
                        }
                    }

                    LoginStatus.Text = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }
    }
}
