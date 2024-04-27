namespace Roadkill.Api.Settings
{
	public class JwtSettings
	{
		public string Password { get; set; }
		public int ExpiresMinutes { get; set; }
		public int RefreshTokenExpiresDays { get; set; }
	}
}
