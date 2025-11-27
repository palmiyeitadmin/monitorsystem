using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Users;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }
    
    public async Task<User?> GetByEmailAsync(string email, bool includeInactive)
    {
        if (includeInactive)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
        
        return await GetByEmailAsync(email);
    }
    
    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
    }
    
    public async Task<User?> GetByEmailVerificationTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }
    
    public async Task<PagedResponse<UserListItemDto>> GetPagedAsync(Guid organizationId, PagedRequest request, string? role = null, bool? isActive = null)
    {
        var query = _dbSet.AsQueryable()
            .Where(u => u.OrganizationId == organizationId);
            
        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<Core.Enums.UserRole>(role, out var userRole))
            {
                query = query.Where(u => u.Role == userRole);
            }
        }
        
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }
        
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }
        
        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "email" => request.SortOrder == "desc" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "fullname" => request.SortOrder == "desc" ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
            "role" => request.SortOrder == "desc" ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "createdat" => request.SortOrder == "desc" ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                AssignedCustomersCount = u.CustomerAssignments.Count,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
            
        return new PagedResponse<UserListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
    
    public async Task<List<User>> GetByOrganizationAsync(Guid organizationId)
    {
        return await _dbSet.Where(u => u.OrganizationId == organizationId).ToListAsync();
    }
    
    public async Task<List<User>> GetByRoleAsync(Guid organizationId, string role)
    {
        if (Enum.TryParse<Core.Enums.UserRole>(role, out var userRole))
        {
            return await _dbSet.Where(u => u.OrganizationId == organizationId && u.Role == userRole).ToListAsync();
        }
        
        return new List<User>();
    }
    
    public async Task<List<User>> GetCustomerUsersAsync(Guid customerId)
    {
        return await _dbSet
            .Where(u => u.CustomerUsers.Any(cu => cu.CustomerId == customerId))
            .ToListAsync();
    }
}
