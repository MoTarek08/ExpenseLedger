namespace Application.Interfaces.HashingService
{
    public interface IHashingService
    {
        public string Hash(string plainText, int workFactor);
        public bool AreEqual(string plainText, string hashed);
    }
}