using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ChainUtils;
using DatabaseLib;
using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser.Messages;
using QRCoder;

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


        

            //var privK = Crypto.GeneratePrivateKey();
            //var pubK = Crypto.GetPublicKey(privK);

            //Console.WriteLine("Private: " + privK);
            //Console.WriteLine("Public: " + pubK);

            //var str = "ahoj";
            //byte[] bytes = Encoding.ASCII.GetBytes(str);

            //var hash = Crypto.SignTransaction(bytes, privK);
            //Console.WriteLine("Signature: " + hash);


            //var list = new List<byte[]>();
            //var arr = hash.ToArray();

            //foreach (var sigVal in arr)
            //{
            //    list.Add(sigVal);
            //}


            //var recoveredKey = Crypto.RecoverPublicKey(list[0], list[1], list[2], bytes);
            //if (StructuralComparisons.StructuralEqualityComparer.Equals(recoveredKey, pubK))
            //{
            //    Console.WriteLine("GOOD JOB");
            //}

            //string s1 = Encoding.UTF8.GetString(privK);
            //string s2 = Encoding.UTF8.GetString(pubK);
            //string s1 = Convert.ToBase64String(privK); // gsjqFw==
            //string s2 = Convert.ToBase64String(pubK); // gsjqFw==


            //Console.WriteLine("Private base64: " + s1);
            //Console.WriteLine("Public base64: " + s2);
            //var x = SHA.GenerateSHA256String(s1);
            //var result = Base58Encoding.Encode(Encoding.ASCII.GetBytes(x));
            //Console.WriteLine("Public base58: " + result);

            //CreateQrCode(s1);


            //other
            //Console.WriteLine("Public: " + serializedPublic2);
            //Console.WriteLine("Private: " + serializedPrivate2);
            //var signature = Crypto.SignData("Traktor", priK);
            //Console.WriteLine("Signature: " + signature);

            //var verificationOk = Crypto.VerifySignature(pubK, signature, "Traktr");
        }


    

        //private void SendMessage(object sender, RoutedEventArgs e)
        //{
        //    TextMessage message = new TextMessage()
        //    {
        //        Text = "Hello"
        //    };
        //    _blockchainNetwork.BroadcastMessage(message);
        //}


    

        //nettwork layer
        public void CreateTransaction()
        {
            //broadcast, when all recieves, add to transaction pool

            //_blockchainNetwork.BroadcastMessageAsync();
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

                        byte[] pk = Encoding.ASCII.GetBytes(newUser.PublicKey);

                        NavigationService?.Navigate(new Login());
                        Reset();
                    }
                }
            }
        }

        private bool SaveToDatabase(User newUser)
        {
            var privK = Crypto.GeneratePrivateKey();
            var pubK = Crypto.GetPublicKey(privK);

            //create file with pub and private key
            File.WriteAllBytes("pub.dat", pubK);
            File.WriteAllBytes("pri.dat", privK);

            newUser.PublicKey = Convert.ToBase64String(pubK);

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
                throw;
            }

            //return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}