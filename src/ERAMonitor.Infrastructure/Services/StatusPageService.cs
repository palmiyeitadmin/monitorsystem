using System.Linq;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPages;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Services;

public class StatusPageService : IStatusPageService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatusPageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<StatusPageDto>> GetPagedAsync(Guid organizationId, PagedRequest request)
    {
        return await _unitOfWork.StatusPages.GetPagedAsync(organizationId, request);
    }

    public async Task<StatusPageDetailDto?> GetDetailAsync(Guid id, Guid organizationId)
    {
        return await _unitOfWork.StatusPages.GetDetailAsync(id, organizationId);
    }

    public async Task<StatusPageDetailDto?> GetBySlugAsync(string slug)
    {
        var statusPage = await _unitOfWork.StatusPages.GetBySlugAsync(slug);
        if (statusPage == null) return null;

        return MapToDetailDto(statusPage);
    }

    public async Task<StatusPageDetailDto> CreateAsync(Guid organizationId, CreateStatusPageDto dto)
    {
        if (string.IsNullOrEmpty(dto.Slug))
        {
            dto.Slug = dto.Name.ToLower().Replace(" ", "-");
        }

        var slugExists = await _unitOfWork.StatusPages.SlugExistsAsync(dto.Slug);
        if (slugExists)
        {
            dto.Slug = $"{dto.Slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        var statusPage = new StatusPage
        {
            OrganizationId = organizationId,
            Name = dto.Name,
            Description = dto.Description,
            Slug = dto.Slug,
            IsPublic = dto.IsPublic,
            IsActive = dto.IsActive,
            CompanyName = dto.CompanyName,
            LogoUrl = dto.LogoUrl,
            FaviconUrl = dto.FaviconUrl,
            WebsiteUrl = dto.WebsiteUrl,
            SupportUrl = dto.SupportUrl,
            CustomCss = dto.CustomCss,
            GoogleAnalyticsId = dto.GoogleAnalyticsId,
            ShowUptime = dto.ShowUptime,
            ShowIncidents = dto.ShowIncidents,
            ShowMaintenances = dto.ShowMaintenance,
            Theme = dto.Theme
        };

        await _unitOfWork.StatusPages.AddAsync(statusPage);
        await _unitOfWork.SaveChangesAsync();

        return await GetDetailAsync(statusPage.Id, organizationId) ?? throw new InvalidOperationException("Failed to retrieve created status page");
    }

    public async Task<StatusPageDetailDto?> UpdateAsync(Guid id, Guid organizationId, UpdateStatusPageDto dto)
    {
        var statusPage = await _unitOfWork.StatusPages.GetByIdAsync(id);
        if (statusPage == null || statusPage.OrganizationId != organizationId) return null;

        statusPage.Name = dto.Name;
        statusPage.Description = dto.Description;
        statusPage.IsPublic = dto.IsPublic;
        statusPage.IsActive = dto.IsActive;
        statusPage.CompanyName = dto.CompanyName;
        statusPage.LogoUrl = dto.LogoUrl;
        statusPage.FaviconUrl = dto.FaviconUrl;
        statusPage.WebsiteUrl = dto.WebsiteUrl;
        statusPage.SupportUrl = dto.SupportUrl;
        statusPage.CustomCss = dto.CustomCss;
        statusPage.GoogleAnalyticsId = dto.GoogleAnalyticsId;
        statusPage.ShowUptime = dto.ShowUptime;
        statusPage.ShowIncidents = dto.ShowIncidents;
        statusPage.ShowMaintenances = dto.ShowMaintenance;
        statusPage.Theme = dto.Theme;

        _unitOfWork.StatusPages.Update(statusPage);
        await _unitOfWork.SaveChangesAsync();

        return await GetDetailAsync(id, organizationId);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid organizationId)
    {
        var statusPage = await _unitOfWork.StatusPages.GetByIdAsync(id);
        if (statusPage == null || statusPage.OrganizationId != organizationId) return false;

        _unitOfWork.StatusPages.Remove(statusPage);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // Component Management
    public async Task<StatusPageComponentDto> AddComponentAsync(Guid statusPageId, Guid organizationId, CreateStatusPageComponentDto dto)
    {
        var statusPage = await _unitOfWork.StatusPages.GetByIdAsync(statusPageId);
        if (statusPage == null || statusPage.OrganizationId != organizationId) throw new KeyNotFoundException("Status Page not found");

        var component = new StatusPageComponent
        {
            StatusPageId = statusPageId,
            GroupId = dto.GroupId,
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Status = StatusPageComponentStatus.Operational,
            ShowUptime = dto.ShowUptime,
            Order = dto.Order,
            ServiceId = dto.ServiceId,
            HostId = dto.HostId
        };

        await _unitOfWork.StatusPageComponents.AddAsync(component);
        await _unitOfWork.SaveChangesAsync();

        return MapToComponentDto(component);
    }

    public async Task<StatusPageComponentDto?> UpdateComponentAsync(Guid componentId, Guid organizationId, UpdateStatusPageComponentDto dto)
    {
        var component = await _unitOfWork.StatusPageComponents.GetByIdAsync(componentId, c => c.StatusPage);
        if (component == null || component.StatusPage.OrganizationId != organizationId) return null;

        component.GroupId = dto.GroupId;
        component.Name = dto.Name;
        component.Description = dto.Description;
        component.Type = dto.Type;
        component.ShowUptime = dto.ShowUptime;
        component.Order = dto.Order;
        component.ServiceId = dto.ServiceId;
        component.HostId = dto.HostId;

        _unitOfWork.StatusPageComponents.Update(component);
        await _unitOfWork.SaveChangesAsync();

        return MapToComponentDto(component);
    }

    public async Task<bool> DeleteComponentAsync(Guid componentId, Guid organizationId)
    {
        var component = await _unitOfWork.StatusPageComponents.GetByIdAsync(componentId, c => c.StatusPage);
        if (component == null || component.StatusPage.OrganizationId != organizationId) return false;

        _unitOfWork.StatusPageComponents.Remove(component);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task UpdateComponentStatusAsync(Guid componentId, Guid organizationId, UpdateComponentStatusDto dto)
    {
        var component = await _unitOfWork.StatusPageComponents.GetByIdAsync(componentId, c => c.StatusPage);
        if (component == null || component.StatusPage.OrganizationId != organizationId) return;

        component.Status = dto.Status;
        _unitOfWork.StatusPageComponents.Update(component);
        await _unitOfWork.SaveChangesAsync();
    }

    // Group Management
    public async Task<StatusPageComponentGroupDto> AddGroupAsync(Guid statusPageId, Guid organizationId, CreateStatusPageComponentGroupDto dto)
    {
        var statusPage = await _unitOfWork.StatusPages.GetByIdAsync(statusPageId);
        if (statusPage == null || statusPage.OrganizationId != organizationId) throw new KeyNotFoundException("Status Page not found");

        var group = new StatusPageComponentGroup
        {
            StatusPageId = statusPageId,
            Name = dto.Name,
            Description = dto.Description,
            SortOrder = dto.Order,
            IsExpanded = !dto.IsCollapsed
        };

        await _unitOfWork.StatusPageComponentGroups.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        return MapToGroupDto(group);
    }

    public async Task<StatusPageComponentGroupDto?> UpdateGroupAsync(Guid groupId, Guid organizationId, UpdateStatusPageComponentGroupDto dto)
    {
        var group = await _unitOfWork.StatusPageComponentGroups.GetByIdAsync(groupId, g => g.StatusPage);
        if (group == null || group.StatusPage.OrganizationId != organizationId) return null;

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.SortOrder = dto.Order;
        group.IsExpanded = !dto.IsCollapsed;

        _unitOfWork.StatusPageComponentGroups.Update(group);
        await _unitOfWork.SaveChangesAsync();

        return MapToGroupDto(group);
    }

    public async Task<bool> DeleteGroupAsync(Guid groupId, Guid organizationId)
    {
        var group = await _unitOfWork.StatusPageComponentGroups.GetByIdAsync(groupId, g => g.StatusPage);
        if (group == null || group.StatusPage.OrganizationId != organizationId) return false;

        _unitOfWork.StatusPageComponentGroups.Remove(group);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // Subscriber Management
    public async Task<PagedResponse<StatusPageSubscriberDto>> GetSubscribersAsync(Guid statusPageId, Guid organizationId, PagedRequest request)
    {
        var statusPage = await _unitOfWork.StatusPages.GetByIdAsync(statusPageId);
        if (statusPage == null || statusPage.OrganizationId != organizationId) throw new KeyNotFoundException("Status Page not found");

        var query = _unitOfWork.StatusPageSubscribers.Query()
            .Where(s => s.StatusPageId == statusPageId);

        var total = query.Count();
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StatusPageSubscriberDto
            {
                Id = s.Id,
                StatusPageId = s.StatusPageId,
                Email = s.Email,
                Phone = s.Phone,
                IsVerified = s.IsVerified,
                SubscribedAt = s.CreatedAt,
                EmailSubscribed = s.EmailSubscribed,
                SmsSubscribed = s.SmsSubscribed
            })
            .ToList();

        return await Task.FromResult(new PagedResponse<StatusPageSubscriberDto>(items, total, request.Page, request.PageSize));
    }

    public async Task<StatusPageSubscriberDto> AddSubscriberAsync(Guid statusPageId, CreateStatusPageSubscriberDto dto)
    {
        var subscriber = new StatusPageSubscriber
        {
            StatusPageId = statusPageId,
            Email = dto.Email ?? string.Empty,
            Phone = dto.Phone,
            EmailSubscribed = dto.EmailSubscribed,
            SmsSubscribed = dto.SmsSubscribed,
            IsVerified = false,
            VerificationToken = Guid.NewGuid().ToString()
        };

        await _unitOfWork.StatusPageSubscribers.AddAsync(subscriber);
        await _unitOfWork.SaveChangesAsync();

        return new StatusPageSubscriberDto
        {
            Id = subscriber.Id,
            StatusPageId = subscriber.StatusPageId,
            Email = subscriber.Email,
            Phone = subscriber.Phone,
            IsVerified = subscriber.IsVerified,
            SubscribedAt = subscriber.CreatedAt,
            EmailSubscribed = subscriber.EmailSubscribed,
            SmsSubscribed = subscriber.SmsSubscribed
        };
    }

    public async Task<bool> RemoveSubscriberAsync(Guid subscriberId, Guid statusPageId, Guid? organizationId = null)
    {
        var subscriber = await _unitOfWork.StatusPageSubscribers.GetByIdAsync(subscriberId, s => s.StatusPage);
        if (subscriber == null || subscriber.StatusPageId != statusPageId) return false;

        if (organizationId.HasValue && subscriber.StatusPage.OrganizationId != organizationId.Value)
            return false;

        _unitOfWork.StatusPageSubscribers.Remove(subscriber);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifySubscriberAsync(Guid subscriberId, string token)
    {
        var subscriber = await _unitOfWork.StatusPageSubscribers.GetByIdAsync(subscriberId);
        if (subscriber == null || subscriber.VerificationToken != token) return false;

        subscriber.IsVerified = true;
        subscriber.VerificationToken = null;
        subscriber.VerifiedAt = DateTime.UtcNow;

        _unitOfWork.StatusPageSubscribers.Update(subscriber);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private StatusPageComponentDto MapToComponentDto(StatusPageComponent component)
    {
        return new StatusPageComponentDto
        {
            Id = component.Id,
            StatusPageId = component.StatusPageId,
            Name = component.Name,
            Description = component.Description,
            Type = component.Type,
            HostId = component.HostId,
            CheckId = component.CheckId,
            ServiceId = component.ServiceId,
            GroupId = component.GroupId,
            SortOrder = component.SortOrder,
            Order = component.Order,
            ShowUptime = component.ShowUptime,
            ShowResponseTime = component.ShowResponseTime,
            Status = component.Status
        };
    }

    private StatusPageComponentGroupDto MapToGroupDto(StatusPageComponentGroup group)
    {
        return new StatusPageComponentGroupDto
        {
            Id = group.Id,
            StatusPageId = group.StatusPageId,
            Name = group.Name,
            Description = group.Description,
            Order = group.SortOrder,
            IsCollapsed = !group.IsExpanded
        };
    }

    private StatusPageDetailDto MapToDetailDto(StatusPage statusPage)
    {
        return new StatusPageDetailDto
        {
            Id = statusPage.Id,
            Name = statusPage.Name,
            Slug = statusPage.Slug,
            CustomDomain = statusPage.CustomDomain,
            LogoUrl = statusPage.LogoUrl,
            FaviconUrl = statusPage.FaviconUrl,
            CompanyName = statusPage.CompanyName,
            PrimaryColor = statusPage.PrimaryColor,
            CustomCss = statusPage.CustomCss,
            HeaderText = statusPage.HeaderText,
            FooterText = statusPage.FooterText,
            AboutText = statusPage.AboutText,
            ShowUptime = statusPage.ShowUptime,
            UptimeDays = statusPage.UptimeDays,
            ShowIncidents = statusPage.ShowIncidents,
            ShowMaintenances = statusPage.ShowMaintenances,
            ShowSubscribe = statusPage.ShowSubscribe,
            ShowResponseTime = statusPage.ShowResponseTime,
            IsPublic = statusPage.IsPublic,
            HasPassword = !string.IsNullOrEmpty(statusPage.Password),
            IsActive = statusPage.IsActive,
            Components = statusPage.Components?.Select(MapToComponentDto).ToList() ?? new List<StatusPageComponentDto>(),
            CreatedAt = statusPage.CreatedAt,
            UpdatedAt = statusPage.UpdatedAt
        };
    }
}
