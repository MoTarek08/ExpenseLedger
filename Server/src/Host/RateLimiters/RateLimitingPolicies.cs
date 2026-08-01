namespace Host.RateLimiters
{
    public static class RateLimitingPolicies
    {
        public const string Login = "login";
        public const string Logout = "logout";
        public const string Upload = "upload";
        public const string RefreshTokens = "refresh-tokens";
        public const string ConcurrentLogin = "conncurrent-server-login";
        public const string ConcurrentUpload = "conncurrent-server-upload";
        public const string ConcurrentRegister = "concurrent-server-register";
        public const string ConcurrentRefreshTokens = "concurrent-server-refresh-tokens";
    }
}
