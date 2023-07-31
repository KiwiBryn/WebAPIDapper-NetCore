using System;
using System.Security.Principal;

namespace devMobile.AspNetCore.Identity.Dapper.Models
{
    public class ApplicationUser : IIdentity
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual string UserName { get; set; }
        public string NormalizedUserName { get; internal set; }
        public virtual string Email { get; set; }
        public string NormalizedEmail{ get; internal set; }
        public virtual bool EmailConfirmed { get; set; }

        public virtual String PasswordHash { get; set; }
        public virtual String SecurityStamp { get; set; }
        public virtual String ConcurrencyStamp { get; set; }
        public virtual String PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        public string? AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public string? Name => throw new NotImplementedException();
    }
}
