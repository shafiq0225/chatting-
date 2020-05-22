using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string Username, string Password)
        {
            //identify the user in our database
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == Username);

            if (user == null)
            {
                return null;
            }
            
            //compare the  given password with passwordHash whether it is matched or not matched
            if (!VerifyPasswordHash(Password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }
            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                //changes the string into byte and them computes the hash
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); //encoding password                
                for (int i = 0; i < computedHash.Length; i++)
                {
                    //compares the hashed password(user input) with the hashed password from the database
                    if (computedHash[i] != passwordHash[i])
                        return false;
                }
                return true;
            }
        }

        public async Task<User> Register(User user, string Password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; //Randomly generate key
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); //encoding password                
            }
        }

        public async Task<bool> UserExists(string Username)
        {
            if (await _context.Users.AnyAsync(x => x.Username == Username))
                return true;

            return false;
        }
    }
}