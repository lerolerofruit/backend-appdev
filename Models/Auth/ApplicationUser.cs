using Microsoft.AspNetCore.Identity;

namespace IMS_API_.Models.Auth;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FullName { get; set; }
}
