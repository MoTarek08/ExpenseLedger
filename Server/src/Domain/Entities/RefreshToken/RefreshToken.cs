using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.RefreshTokenNamespace
{
    public class RefreshToken
    {

        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid SessionId { get; private set; }

        public string Token { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }

        public User User { get; private set; } = null!;

        private RefreshToken() { }

        private RefreshToken(
            Guid userId,
            Guid sessionId,
            string token,
            DateTimeOffset generatedAt,
            DateTimeOffset expiresAt)
        {
            Id = Guid.NewGuid();
            SessionId = sessionId;
            UserId = userId;
            Token = token;
            CreatedAt = generatedAt;
            ExpiresAt = expiresAt;
        }

        public static RefreshToken Create(
            Guid userId,
            Guid sessionId,
            string token,
            DateTimeOffset generatedAt,
            DateTimeOffset expiresAt)
        {

            if (userId == Guid.Empty)
                throw new DomainException("User id is required");

            if (string.IsNullOrWhiteSpace(token))
                throw new DomainException("Encrypted token is required");

            if (generatedAt > expiresAt)
                throw new DomainException("Token expiry date cannot be earlier than token generation date");

            return new RefreshToken(userId, sessionId, token, generatedAt, expiresAt);
        }


        public RefreshToken Revoke(DateTimeOffset revokedAt)
        {
            if (revokedAt < CreatedAt)
                throw new DomainException("Token revoking date cannot be earlier than the creation date");

            RevokedAt = revokedAt;
            return this;
        }

        public bool IsExpiredIn(DateTimeOffset date)
        {
            return date >= ExpiresAt;
        }

    }
}
