﻿using Microsoft.AspNetCore.Authorization;

public class OwnershipRequirement : IAuthorizationRequirement
{
    public OwnershipRequirement()
    {
    }
}
