namespace MovieAPI.Domain.Constants;

public static class CustomClaimTypes
{
  // Embedded in every access token at issuance and re-checked against the user's
  // current value on every authenticated request - see AddJwtBearer's
  // OnTokenValidated handler in Program.cs. Anything that changes the stored
  // SecurityStamp (password change/reset) immediately invalidates tokens issued
  // before that point, without needing a token blacklist.
  public const string SecurityStamp = "security_stamp";
}
