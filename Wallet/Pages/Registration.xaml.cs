using CoreLib;
using DatabaseLib;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Wallet.Pages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Registration : Page
    {
        public Registration()
        {
            InitializeComponent();

        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        public void Reset()
        {
            TextBoxFirstName.Text = "";
            TextBoxLastName.Text = "";
            TextBoxEmail.Text = "";
            PasswordBox1.Password = "";
            PasswordBoxConfirm.Password = "";
        }


        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxEmail.Text.Length == 0)
            {
                Errormessage.Text = "Enter an email.";
                TextBoxEmail.Focus();
            }
            else if (!Regex.IsMatch(TextBoxEmail.Text,
                @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                Errormessage.Text = "Enter a valid email.";
                TextBoxEmail.Select(0, TextBoxEmail.Text.Length);
                TextBoxEmail.Focus();
            }
            else
            {
                string firstname = TextBoxFirstName.Text;
                string lastname = TextBoxLastName.Text;
                string email = TextBoxEmail.Text;
                string password = PasswordBox1.Password;
                if (PasswordBox1.Password.Length == 0)
                {
                    Errormessage.Text = "Enter password.";
                    PasswordBox1.Focus();
                }
                else if (PasswordBoxConfirm.Password.Length == 0)
                {
                    Errormessage.Text = "Enter Confirm password.";
                    PasswordBoxConfirm.Focus();
                }
                else if (PasswordBox1.Password != PasswordBoxConfirm.Password)
                {
                    Errormessage.Text = "Confirm password must be same as password.";
                    PasswordBoxConfirm.Focus();
                }
                else
                {
                    Errormessage.Text = "";
                    User newUser = new User()
                    {
                        Pass = password,
                        Email = email,
                        FirstName = firstname,
                        LastName = lastname,
                        RegistrationDate = DateTime.Now
                    };


                    if (SaveToDatabase(newUser))
                    {
                        Errormessage.Text = "You have Registered successfully.";
                        RedirectToLoginPage(newUser);
                        Reset();
                    }
                }
            }
        }

        private void RedirectToLoginPage(User newUser)
        {
            NavigationService?.Navigate(new Login(newUser));
        }

        private bool SaveToDatabase(User newUser)
        {

            WalletBank wBank = new WalletBank();
            var wallet = wBank.CreateWallet(newUser.Password);

            newUser.PublicKey = Convert.ToBase64String(wallet.PublicKey);
            newUser.PublicKeyHashed = Convert.ToBase64String(wallet.PublicKeyHash);
            newUser.Address= wallet.Address;

           
            //save to Database
            UserContext context = new UserContext();
            context.Users.Add(newUser);
            try
            {
                context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Loading());
        }
    }
}