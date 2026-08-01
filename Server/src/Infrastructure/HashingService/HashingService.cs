using Application.Interfaces.HashingService;

namespace Infrastructure.HashingServiceNamespace
{
    public class HashingService : IHashingService
    {
        public string Hash(string plainText, int workFactor)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(plainText, workFactor, BCrypt.Net.HashType.SHA384);
        }

        public bool AreEqual(string plainText, string hashed)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(plainText, hashed, BCrypt.Net.HashType.SHA384);
        }
    }
}
