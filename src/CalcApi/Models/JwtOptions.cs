namespace CalcApi
{
    public sealed class JwtOptions
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = "CalcApi";
        public string Audience { get; set; } = "CalcApiClients";
        public int ExpiryMinutes { get; set; } = 15;
    }
}
