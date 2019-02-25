using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using ChainUtils;

namespace DatabaseLib
{
    public class User
    {
        [Key] public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PublicKey { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Password { get; set; }

        [NotMapped]
        public string Pass
        {
            get => Password;
            set
            {
                Password = Sha.GenerateSha256String(value);
            }
        }
    }
}