PHASE 2: Authentication & Core API (Days 4-7)2.1 Authentication SystemImplement JWT authentication with refresh tokens:Features:
- User registration (admin only)
- Login with email/password
- Login with Microsoft SSO (optional)
- JWT access tokens (15 min expiry)
- Refresh tokens (7 days expiry)
- Password reset flow
- Email verification
- Session management (active sessions list, revoke sessions)
- Role-based authorization
- Permission-based authorization2.2 API Endpoints - Phase 2Authentication:
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
GET    /api/auth/me
PUT    /api/auth/profile
PUT    /api/auth/change-password
GET    /api/auth/sessions
DELETE /api/auth/sessions/{id}

Users (Admin):
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
PUT    /api/users/{id}/permissions

Customers:
GET    /api/customers
GET    /api/customers/{id}
POST   /api/customers
PUT    /api/customers/{id}
DELETE /api/customers/{id}
GET    /api/customers/{id}/resources

Locations:
GET    /api/locations
GET    /api/locations/{id}
POST   /api/locations
PUT    /api/locations/{id}
DELETE /api/locations/{id}2.3 Middleware & ServicesImplement:
- Exception handling middleware
- Request logging middleware
- Rate limiting middleware
- Tenant resolution middleware (multi-tenancy)
- Audit logging service
- Email service (SMTP)
- Password hashing service (BCrypt)