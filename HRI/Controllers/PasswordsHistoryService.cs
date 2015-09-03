using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HRI.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace HRI.Controllers
{
    public class PasswordsHistoryService
    {
        private readonly UmbracoDatabase _dbContext = ApplicationContext.Current.DatabaseContext.Database;

        /// <summary>
        /// Check users last passwords
        /// </summary>
        /// <param name="memberId">Member Id</param>
        /// <param name="password">Password</param>
        /// <returns>Search result</returns>
        public bool CheckUserPassword(int memberId, string password)
        {
            //Get last 3 passwords from the history
            var lastPasswords = _dbContext.Query<PasswordHistory>("SELECT TOP 3 * FROM PasswordsHistory WHERE MemberId=@0 ORDER BY ChangeDate DESC ", memberId).ToList();
            var result = lastPasswords.All(p => p.EncryptedPassword != HashPassword(password));
            return result;
        }

        /// <summary>
        /// Add user password hash to the history
        /// </summary>
        /// <param name="memberId">Member Id</param>
        /// <param name="password">Password</param>
        public void Add(int memberId, string password)
        {
            var historyItem = new PasswordHistory()
            {
                MemberId = memberId,
                EncryptedPassword = HashPassword(password),
                ChangeDate = DateTime.Now
            };
            _dbContext.Insert(historyItem);
        }

        /// <summary>
        /// Compute password hash
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Encrypted password</returns>
        private string HashPassword(string password)
        {
            // Prepare password hash
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashedPassword = new SHA1CryptoServiceProvider().ComputeHash(passwordBytes);
            var encryptedPassword = Encoding.UTF8.GetString(hashedPassword);

            return encryptedPassword;
        }
    }
}