﻿using System;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class AuthenticatedUserInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? ProviderId { get; set; }
        public string Role { get; set; }

        public bool IsDeveloper => Role == RoleNames.Developer;
        public bool IsHelpdesk => Role == RoleNames.Helpdesk;
        public bool IsProvider => Role == RoleNames.ProviderSuperUser || Role == RoleNames.ProviderUser;
    }
}
