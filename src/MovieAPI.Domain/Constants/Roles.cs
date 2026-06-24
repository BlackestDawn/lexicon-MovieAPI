namespace MovieAPI.Domain.Constants;

public static class Roles
{
  public const string User = "User";
  public const string PowerUser = "PowerUser";
  public const string Moderator = "Moderator";
  public const string Administrator = "Administrator";

  public const string PowerUserAndAbove = "PowerUser,Moderator,Administrator";
  public const string ModeratorAndAbove = "Moderator,Administrator";

  public static readonly string[] All = [User, PowerUser, Moderator, Administrator];
}
