﻿using ChainUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseLib
{
    public class User
    {
        [Key] public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PublicKey { get; set; }
        public string PublicKeyHashed { get; set; }
        public string Address { get; set; }
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