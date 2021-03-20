using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace moo.common.Models
{
    public class HumanPlayer : Player
    {
        public string? passwordSaltBase64; // protected for deserialization
        public string? passwordHashBase64; // protected for deserialization

        public static HumanPlayer make(string name, Thing location) // Do not correct casing, will impact GetMethod("Make")
        {
            var player = ThingRepository.Instance.Make<HumanPlayer>();
            player.name = name;
            player.Home = location.id;
            player.Location = location.id;
            return player;
        }
        protected override Dictionary<string, object?> GetSerializedElements()
        {
            var result = base.GetSerializedElements();
            result.Add(nameof(passwordSaltBase64), passwordSaltBase64);
            result.Add(nameof(passwordHashBase64), passwordHashBase64);
            return result;
        }

        public bool CheckPassword(string testPassword)
        {
            if (string.IsNullOrWhiteSpace(testPassword))
                return false;
            if (passwordSaltBase64 == null)
                throw new InvalidOperationException($"No password salt is present on the player {id}");
            if (passwordHashBase64 == null)
                throw new InvalidOperationException($"No password hash is present on the player {id}");

            var salt = Convert.FromBase64String(passwordSaltBase64);
            var realHash = Convert.FromBase64String(passwordHashBase64);

            // Generate the test hash
            using var pbkdf2 = new Rfc2898DeriveBytes(testPassword, salt, iterations: 10000);
            var testHash = pbkdf2.GetBytes(20); //20 bytes length is 160 bits
            return testHash.SequenceEqual(realHash);
        }

        public bool SetPassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return false;

            // Generate a salt
            // generate a 128-bit salt using a secure PRNG
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generate the hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(newPassword, salt, iterations: 10000))
            {
                var hash = pbkdf2.GetBytes(20); //20 bytes length is 160 bits
                passwordSaltBase64 = Convert.ToBase64String(salt);
                passwordHashBase64 = Convert.ToBase64String(hash);
            }

            Dirty = true;
            return true;
        }
    }
}