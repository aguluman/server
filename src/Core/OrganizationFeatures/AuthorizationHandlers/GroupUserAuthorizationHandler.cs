﻿using Bit.Core.Context;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Bit.Core.OrganizationFeatures.AuthorizationHandlers;

class GroupUserAuthorizationHandler : AuthorizationHandler<GroupUserOperationRequirement, GroupUser>
{
    private readonly ICurrentContext _currentContext;
    private readonly IGroupRepository _groupRepository;

    public GroupUserAuthorizationHandler(ICurrentContext currentContext, IGroupRepository groupRepository)
    {
        _currentContext = currentContext;
        _groupRepository = groupRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        GroupUserOperationRequirement requirement,
        GroupUser resource)
    {
        if (resource == null)
        {
            context.Fail();
            return;
        }

        switch (requirement)
        {
            case not null when requirement == GroupUserOperations.Create:
                CanCreate(context, requirement, resource);
                break;

            case not null when requirement == GroupUserOperations.Delete:
                CanDelete(context, requirement, resource);
                break;
        }

        await Task.CompletedTask;
    }

    private async Task CanCreate(AuthorizationHandlerContext context, GroupUserOperationRequirement requirement,
        GroupUser resource)
    {
        CanManage(context, requirement, resource);
    }

    private async Task CanDelete(AuthorizationHandlerContext context, GroupUserOperationRequirement requirement,
        GroupUser resource)
    {
        CanManage(context, requirement, resource);
    }

    private async Task CanManage(AuthorizationHandlerContext context, GroupUserOperationRequirement requirement,
        GroupUser resource)
    {
        var group = await _groupRepository.GetByIdAsync(resource.GroupId);
        if (group == null)
        {
            context.Fail();
            return;
        }

        // TODO: providers need to be included in the claims
        var org = _currentContext.GetOrganization(group.OrganizationId);
        if (org == null)
        {
            context.Fail();
        }

        var canAccess = org.Type == OrganizationUserType.Owner ||
                        org.Type == OrganizationUserType.Admin ||
                        org.Permissions.ManageGroups;

        if (canAccess)
        {
            context.Succeed(requirement);
        }
    }
}