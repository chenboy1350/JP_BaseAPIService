namespace SimpleJWT.Model
{
    public class AuthModel
    {
        public string? Header { get; set; }
    }

    public class TryAuthModel
    {
        public string? ClientId { get; set; }

        public string? ClientSecret { get; set; }
    }
}
