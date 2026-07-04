using Microsoft.AspNetCore.Identity;
using Moq;
using MovieAPI.Domain.Entities;

namespace MovieAPI.UnitTests;

// UserManager<TUser> has no parameterless constructor and most of its dependencies
// are irrelevant once every method we call is individually Setup() on the mock, so
// nulls are safe here - the real constructor only null-coalesces them into defaults.
internal static class IdentityMocks
{
  public static Mock<UserManager<ApplicationUser>> MockUserManager()
  {
    var store = new Mock<IUserStore<ApplicationUser>>();
    return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
  }
}
