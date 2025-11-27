using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }
}
