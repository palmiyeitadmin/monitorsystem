-- =============================================
-- ERA MONITOR DATABASE SCHEMA
-- PostgreSQL 15+
-- =============================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =============================================
-- ENUMS
-- =============================================

CREATE TYPE user_role AS ENUM ('SuperAdmin', 'Admin', 'Operator', 'Viewer', 'CustomerUser');
CREATE TYPE os_type AS ENUM ('Windows', 'Linux');
CREATE TYPE host_category AS ENUM ('PhysicalServer', 'VirtualMachine', 'VPS', 'DedicatedServer', 'CloudInstance');
CREATE TYPE location_category AS ENUM ('Colocation', 'CloudProvider', 'HostingProvider', 'OnPremise');
CREATE TYPE check_type AS ENUM ('HTTP', 'TCP', 'Ping', 'DNS', 'CustomHealth');
CREATE TYPE status_type AS ENUM ('Up', 'Down', 'Warning', 'Degraded', 'Unknown', 'Disabled');
CREATE TYPE incident_status AS ENUM ('New', 'Acknowledged', 'InProgress', 'Resolved', 'Closed');
CREATE TYPE incident_severity AS ENUM ('Critical', 'High', 'Medium', 'Low', 'Info');
CREATE TYPE notification_channel AS ENUM ('Email', 'SMS', 'Telegram', 'Webhook');
CREATE TYPE notification_status AS ENUM ('Pending', 'Sent', 'Delivered', 'Failed');
CREATE TYPE service_type AS ENUM ('IIS_Site', 'IIS_AppPool', 'WindowsService', 'SystemdUnit', 'DockerContainer', 'Process');

-- =============================================
-- CORE TABLES
-- =============================================

-- Organizations/Tenants (for multi-tenancy support)
CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    logo_url TEXT,
    settings JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Users
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255), -- NULL if using SSO
    full_name VARCHAR(200) NOT NULL,
    avatar_url TEXT,
    phone VARCHAR(50),
    role user_role NOT NULL DEFAULT 'Viewer',
    permissions JSONB DEFAULT '{}',
    notification_preferences JSONB DEFAULT '{"email": true, "sms": false, "telegram": false}',
    timezone VARCHAR(50) DEFAULT 'UTC',
    is_active BOOLEAN DEFAULT true,
    email_verified BOOLEAN DEFAULT false,
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- User Sessions (for active sessions tracking)
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL,
    device_info VARCHAR(500),
    ip_address INET,
    location VARCHAR(200),
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    last_active_at TIMESTAMPTZ DEFAULT NOW()
);

-- Customers (Companies being monitored)
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    logo_url TEXT,
    industry VARCHAR(100),
    
    -- Primary Contact
    contact_name VARCHAR(200),
    contact_email VARCHAR(255),
    contact_phone VARCHAR(50),
    contact_mobile VARCHAR(50),
    contact_job_title VARCHAR(100),
    
    -- Secondary Contact
    secondary_contact_name VARCHAR(200),
    secondary_contact_email VARCHAR(255),
    secondary_contact_phone VARCHAR(50),
    
    -- Emergency Contact
    emergency_contact_name VARCHAR(200),
    emergency_contact_phone VARCHAR(50),
    emergency_available_hours VARCHAR(50), -- e.g., "24/7" or "09:00-18:00"
    
    -- Address
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    country VARCHAR(100),
    postal_code VARCHAR(20),
    
    -- Notification Settings
    notification_settings JSONB DEFAULT '{
        "channels": {"email": true, "sms": false, "telegram": false, "webhook": false},
        "email_recipients": [],
        "sms_numbers": [],
        "telegram_chat_id": null,
        "webhook_url": null,
        "notify_on": {
            "host_down": true,
            "service_stopped": true,
            "high_cpu": true,
            "high_ram": true,
            "disk_critical": true,
            "ssl_expiring": true,
            "all_incidents": false,
            "weekly_summary": false
        },
        "quiet_hours": {
            "enabled": false,
            "from": "22:00",
            "to": "07:00",
            "timezone": "UTC"
        }
    }',
    
    -- Portal Access
    portal_enabled BOOLEAN DEFAULT true,
    api_enabled BOOLEAN DEFAULT false,
    api_key VARCHAR(64) UNIQUE,
    api_rate_limit INT DEFAULT 1000, -- requests per hour
    
    -- Assigned Admin
    assigned_admin_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    notes TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    UNIQUE(organization_id, slug)
);

-- Customer Users (Portal users for customers)
CREATE TABLE customer_users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID REFERENCES customers(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    is_primary BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    
    UNIQUE(customer_id, user_id)
);

-- User-Customer Assignments (for internal users)
CREATE TABLE user_customer_assignments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    
    UNIQUE(user_id, customer_id)
);

-- Locations/Datacenters
CREATE TABLE locations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    category location_category NOT NULL,
    provider_name VARCHAR(200),
    city VARCHAR(100),
    country VARCHAR(100),
    address TEXT,
    contact_info TEXT,
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    notes TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Hosts (Servers)
CREATE TABLE hosts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    location_id UUID REFERENCES locations(id) ON DELETE SET NULL,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    name VARCHAR(200) NOT NULL,
    hostname VARCHAR(255),
    description TEXT,
    os_type os_type NOT NULL,
    os_version VARCHAR(100),
    category host_category NOT NULL DEFAULT 'VirtualMachine',
    tags TEXT[] DEFAULT '{}',
    
    -- Agent Configuration
    api_key VARCHAR(64) UNIQUE NOT NULL DEFAULT encode(gen_random_bytes(32), 'hex'),
    agent_version VARCHAR(20),
    check_interval_seconds INT DEFAULT 60,
    
    -- Current Status (updated by agent)
    current_status status_type DEFAULT 'Unknown',
    last_seen_at TIMESTAMPTZ,
    last_heartbeat JSONB, -- Latest metrics snapshot
    
    -- Computed/Cached Values
    uptime_seconds BIGINT,
    cpu_percent DECIMAL(5,2),
    ram_percent DECIMAL(5,2),
    ram_used_mb BIGINT,
    ram_total_mb BIGINT,
    
    -- Settings
    monitoring_enabled BOOLEAN DEFAULT true,
    alert_on_down BOOLEAN DEFAULT true,
    alert_delay_seconds INT DEFAULT 60,
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Host Disks
CREATE TABLE host_disks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    host_id UUID REFERENCES hosts(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL, -- "C:", "/dev/sda1"
    mount_point VARCHAR(255),
    total_gb DECIMAL(10,2),
    used_gb DECIMAL(10,2),
    used_percent DECIMAL(5,2),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    UNIQUE(host_id, name)
);

-- Host Metrics (Time series data)
CREATE TABLE host_metrics (
    id BIGSERIAL PRIMARY KEY,
    host_id UUID REFERENCES hosts(id) ON DELETE CASCADE,
    cpu_percent DECIMAL(5,2),
    ram_percent DECIMAL(5,2),
    ram_used_mb BIGINT,
    ram_total_mb BIGINT,
    disk_info JSONB, -- [{name, totalGb, usedGb, usedPercent}]
    network_in_bytes BIGINT,
    network_out_bytes BIGINT,
    uptime_seconds BIGINT,
    process_count INT,
    recorded_at TIMESTAMPTZ DEFAULT NOW()
);

-- Services (IIS, Windows Services, Systemd, Docker)
CREATE TABLE services (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    host_id UUID REFERENCES hosts(id) ON DELETE CASCADE,
    service_type service_type NOT NULL,
    service_name VARCHAR(200) NOT NULL, -- Internal name: "w3svc", "nginx.service"
    display_name VARCHAR(300), -- Friendly name
    description TEXT,
    
    -- Current Status
    current_status status_type DEFAULT 'Unknown',
    last_status_change TIMESTAMPTZ,
    
    -- Service-specific Config (varies by type)
    config JSONB DEFAULT '{}',
    -- For IIS: {siteName, appPoolName, bindings, physicalPath}
    -- For Windows Service: {startType, serviceAccount, executablePath}
    -- For Systemd: {unitFile, activeState, subState}
    -- For Docker: {containerId, image, ports}
    
    -- Monitoring
    monitoring_enabled BOOLEAN DEFAULT true,
    restart_count INT DEFAULT 0,
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    UNIQUE(host_id, service_type, service_name)
);

-- Service Status History
CREATE TABLE service_status_history (
    id BIGSERIAL PRIMARY KEY,
    service_id UUID REFERENCES services(id) ON DELETE CASCADE,
    status status_type NOT NULL,
    message TEXT,
    recorded_at TIMESTAMPTZ DEFAULT NOW()
);

-- Checks (HTTP, TCP, Ping, DNS, Custom Health)
CREATE TABLE checks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    host_id UUID REFERENCES hosts(id) ON DELETE SET NULL, -- Optional association
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    location_id UUID REFERENCES locations(id) ON DELETE SET NULL,
    
    name VARCHAR(200) NOT NULL,
    check_type check_type NOT NULL,
    
    -- Target Configuration
    target VARCHAR(500) NOT NULL, -- URL for HTTP, host:port for TCP
    
    -- HTTP-specific
    http_method VARCHAR(10) DEFAULT 'GET',
    expected_status_code INT DEFAULT 200,
    expected_keyword VARCHAR(500),
    keyword_should_exist BOOLEAN DEFAULT true,
    request_headers JSONB DEFAULT '{}',
    request_body TEXT,
    follow_redirects BOOLEAN DEFAULT true,
    
    -- TCP-specific
    tcp_port INT,
    send_data TEXT,
    
    -- SSL Monitoring
    monitor_ssl BOOLEAN DEFAULT true,
    ssl_expiry_warning_days INT DEFAULT 14,
    
    -- Timing
    timeout_seconds INT DEFAULT 30,
    interval_seconds INT DEFAULT 60,
    
    -- Current Status
    current_status status_type DEFAULT 'Unknown',
    last_check_at TIMESTAMPTZ,
    last_response_time_ms INT,
    last_status_code INT,
    last_error_message TEXT,
    ssl_expiry_date DATE,
    ssl_days_remaining INT,
    
    monitoring_enabled BOOLEAN DEFAULT true,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Check Results (History)
CREATE TABLE check_results (
    id BIGSERIAL PRIMARY KEY,
    check_id UUID REFERENCES checks(id) ON DELETE CASCADE,
    status status_type NOT NULL,
    response_time_ms INT,
    status_code INT,
    error_message TEXT,
    response_body_preview TEXT, -- First 500 chars
    ssl_expiry_date DATE,
    ssl_days_remaining INT,
    headers JSONB,
    checked_at TIMESTAMPTZ DEFAULT NOW()
);

-- =============================================
-- INCIDENTS
-- =============================================

CREATE TABLE incidents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    -- Incident Info
    incident_number SERIAL, -- Human-readable: INC-00001
    title VARCHAR(500) NOT NULL,
    description TEXT,
    
    -- Status & Severity
    status incident_status DEFAULT 'New',
    severity incident_severity DEFAULT 'Medium',
    priority VARCHAR(10) DEFAULT 'P3', -- P1, P2, P3, P4
    impact VARCHAR(50), -- 'AllUsers', 'MultipleUsers', 'SingleUser', 'NoUsers'
    
    -- Source
    source_type VARCHAR(50), -- 'Host', 'Service', 'Check'
    source_id UUID,
    
    -- Assignment
    assigned_to_id UUID REFERENCES users(id) ON DELETE SET NULL,
    acknowledged_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    resolved_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    -- Timestamps
    acknowledged_at TIMESTAMPTZ,
    resolved_at TIMESTAMPTZ,
    closed_at TIMESTAMPTZ,
    
    -- SLA
    response_sla_minutes INT DEFAULT 15,
    resolution_sla_minutes INT DEFAULT 240,
    response_sla_met BOOLEAN,
    resolution_sla_met BOOLEAN,
    
    -- Resolution
    root_cause_category VARCHAR(100),
    root_cause_description TEXT,
    resolution_steps TEXT,
    preventive_actions TEXT,
    
    -- Affected Resources (denormalized for quick access)
    affected_resources JSONB DEFAULT '[]',
    -- [{type: 'Host', id: 'uuid', name: 'PROD-WEB-01', status: 'Down'}]
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Incident Timeline/Comments
CREATE TABLE incident_timeline (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    incident_id UUID REFERENCES incidents(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    event_type VARCHAR(50) NOT NULL,
    -- 'Created', 'Acknowledged', 'Assigned', 'StatusChanged', 'NoteAdded', 
    -- 'NotificationSent', 'Escalated', 'Resolved', 'Closed'
    
    content TEXT,
    is_internal BOOLEAN DEFAULT true, -- false = visible to customer
    metadata JSONB DEFAULT '{}',
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Incident-Resource Associations
CREATE TABLE incident_resources (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    incident_id UUID REFERENCES incidents(id) ON DELETE CASCADE,
    resource_type VARCHAR(50) NOT NULL, -- 'Host', 'Service', 'Check'
    resource_id UUID NOT NULL,
    is_primary BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- =============================================
-- NOTIFICATIONS
-- =============================================

CREATE TABLE notification_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    
    -- Condition
    condition_type VARCHAR(50) NOT NULL, -- 'HostDown', 'ServiceStopped', 'HighCPU', 'SSLExpiring', etc.
    condition_config JSONB DEFAULT '{}', -- {threshold: 90, duration_minutes: 5}
    
    -- Target
    notify_user_ids UUID[] DEFAULT '{}',
    notify_customer BOOLEAN DEFAULT false,
    channel notification_channel NOT NULL,
    
    -- Escalation
    escalation_after_minutes INT,
    escalation_user_ids UUID[] DEFAULT '{}',
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    
    -- What triggered this
    trigger_type VARCHAR(50), -- 'Incident', 'Check', 'Host', 'Scheduled'
    trigger_id UUID,
    rule_id UUID REFERENCES notification_rules(id) ON DELETE SET NULL,
    
    -- Notification Details
    channel notification_channel NOT NULL,
    recipient VARCHAR(500) NOT NULL, -- email, phone, chat_id, url
    subject VARCHAR(500),
    content TEXT NOT NULL,
    content_html TEXT,
    
    -- Delivery Status
    status notification_status DEFAULT 'Pending',
    sent_at TIMESTAMPTZ,
    delivered_at TIMESTAMPTZ,
    error_message TEXT,
    retry_count INT DEFAULT 0,
    next_retry_at TIMESTAMPTZ,
    
    -- External IDs
    external_message_id VARCHAR(255),
    
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- =============================================
-- REPORTS
-- =============================================

CREATE TABLE reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    name VARCHAR(200) NOT NULL,
    report_type VARCHAR(50) NOT NULL, -- 'Uptime', 'Incidents', 'Performance', 'Security'
    
    -- Parameters
    parameters JSONB DEFAULT '{}',
    -- {host_ids: [], check_ids: [], date_from, date_to}
    
    -- Generated File
    file_url TEXT,
    file_format VARCHAR(10), -- 'PDF', 'XLSX', 'CSV'
    file_size_bytes BIGINT,
    
    -- Status
    status VARCHAR(20) DEFAULT 'Pending', -- 'Pending', 'Generating', 'Ready', 'Failed'
    error_message TEXT,
    
    generated_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    generated_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE scheduled_reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    name VARCHAR(200) NOT NULL,
    report_type VARCHAR(50) NOT NULL,
    parameters JSONB DEFAULT '{}',
    
    -- Schedule (CRON expression or simple interval)
    schedule_cron VARCHAR(100), -- "0 8 * * 1" (Every Monday 8 AM)
    schedule_timezone VARCHAR(50) DEFAULT 'UTC',
    
    -- Delivery
    delivery_emails TEXT[] DEFAULT '{}',
    delivery_webhook_url TEXT,
    
    last_run_at TIMESTAMPTZ,
    next_run_at TIMESTAMPTZ,
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- =============================================
-- AUDIT LOG
-- =============================================

CREATE TABLE audit_logs (
    id BIGSERIAL PRIMARY KEY,
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    action VARCHAR(100) NOT NULL, -- 'Create', 'Update', 'Delete', 'Login', 'Logout'
    entity_type VARCHAR(100) NOT NULL, -- 'Host', 'Customer', 'User', etc.
    entity_id UUID,
    entity_name VARCHAR(255),
    
    old_values JSONB,
    new_values JSONB,
    
    ip_address INET,
    user_agent TEXT,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- =============================================
-- SYSTEM SETTINGS
-- =============================================

CREATE TABLE system_settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    
    -- Email Settings
    smtp_host VARCHAR(255),
    smtp_port INT DEFAULT 587,
    smtp_username VARCHAR(255),
    smtp_password_encrypted TEXT,
    smtp_from_email VARCHAR(255),
    smtp_from_name VARCHAR(100),
    smtp_use_ssl BOOLEAN DEFAULT true,
    
    -- Telegram Settings
    telegram_bot_token_encrypted TEXT,
    
    -- General Settings
    default_check_interval_seconds INT DEFAULT 60,
    default_alert_delay_seconds INT DEFAULT 60,
    retention_days_metrics INT DEFAULT 30,
    retention_days_logs INT DEFAULT 90,
    
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- =============================================
-- INDEXES
-- =============================================

-- Users
CREATE INDEX idx_users_organization ON users(organization_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);

-- Customers
CREATE INDEX idx_customers_organization ON customers(organization_id);
CREATE INDEX idx_customers_assigned_admin ON customers(assigned_admin_id);
CREATE INDEX idx_customers_api_key ON customers(api_key);

-- Hosts
CREATE INDEX idx_hosts_organization ON hosts(organization_id);
CREATE INDEX idx_hosts_customer ON hosts(customer_id);
CREATE INDEX idx_hosts_location ON hosts(location_id);
CREATE INDEX idx_hosts_api_key ON hosts(api_key);
CREATE INDEX idx_hosts_status ON hosts(current_status);
CREATE INDEX idx_hosts_last_seen ON hosts(last_seen_at DESC);

-- Host Metrics (Time series optimization)
CREATE INDEX idx_host_metrics_host_time ON host_metrics(host_id, recorded_at DESC);
CREATE INDEX idx_host_metrics_recorded ON host_metrics(recorded_at DESC);

-- Services
CREATE INDEX idx_services_host ON services(host_id);
CREATE INDEX idx_services_status ON services(current_status);

-- Service Status History
CREATE INDEX idx_service_status_history_service_time ON service_status_history(service_id, recorded_at DESC);

-- Checks
CREATE INDEX idx_checks_organization ON checks(organization_id);
CREATE INDEX idx_checks_customer ON checks(customer_id);
CREATE INDEX idx_checks_host ON checks(host_id);
CREATE INDEX idx_checks_status ON checks(current_status);
CREATE INDEX idx_checks_type ON checks(check_type);

-- Check Results
CREATE INDEX idx_check_results_check_time ON check_results(check_id, checked_at DESC);
CREATE INDEX idx_check_results_checked ON check_results(checked_at DESC);

-- Incidents
CREATE INDEX idx_incidents_organization ON incidents(organization_id);
CREATE INDEX idx_incidents_customer ON incidents(customer_id);
CREATE INDEX idx_incidents_status ON incidents(status);
CREATE INDEX idx_incidents_severity ON incidents(severity);
CREATE INDEX idx_incidents_assigned ON incidents(assigned_to_id);
CREATE INDEX idx_incidents_created ON incidents(created_at DESC);

-- Incident Timeline
CREATE INDEX idx_incident_timeline_incident ON incident_timeline(incident_id, created_at DESC);

-- Notifications
CREATE INDEX idx_notifications_organization ON notifications(organization_id);
CREATE INDEX idx_notifications_status ON notifications(status);
CREATE INDEX idx_notifications_created ON notifications(created_at DESC);

-- Audit Logs
CREATE INDEX idx_audit_logs_organization ON audit_logs(organization_id);
CREATE INDEX idx_audit_logs_user ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_created ON audit_logs(created_at DESC);

-- =============================================
-- PARTITIONING (Optional - for large datasets)
-- =============================================

-- For host_metrics, check_results, and audit_logs, consider:
-- - Monthly partitioning for metrics (range on recorded_at)
-- - Automatic partition management with pg_partman extension

-- =============================================
-- TRIGGERS FOR UPDATED_AT
-- =============================================

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to all tables with updated_at
CREATE TRIGGER update_organizations_updated_at BEFORE UPDATE ON organizations FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_customers_updated_at BEFORE UPDATE ON customers FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_locations_updated_at BEFORE UPDATE ON locations FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_hosts_updated_at BEFORE UPDATE ON hosts FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_services_updated_at BEFORE UPDATE ON services FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_checks_updated_at BEFORE UPDATE ON checks FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_incidents_updated_at BEFORE UPDATE ON incidents FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_notification_rules_updated_at BEFORE UPDATE ON notification_rules FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_scheduled_reports_updated_at BEFORE UPDATE ON scheduled_reports FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- SEED DATA
-- =============================================

-- Default Organization
INSERT INTO organizations (id, name, slug) VALUES 
    ('00000000-0000-0000-0000-000000000001', 'ERA Cloud', 'era-cloud');

-- Super Admin User (password: Admin123!)
INSERT INTO users (organization_id, email, password_hash, full_name, role) VALUES 
    ('00000000-0000-0000-0000-000000000001', 'admin@eracloud.com.tr', 
     '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S3rgGC6Y9.P5Aq', 
     'System Administrator', 'SuperAdmin');

-- Default System Settings
INSERT INTO system_settings (organization_id) VALUES 
    ('00000000-0000-0000-0000-000000000001');

-- Sample Locations
INSERT INTO locations (organization_id, name, category, provider_name, city, country) VALUES 
    ('00000000-0000-0000-0000-000000000001', 'Turkcell Datacenter', 'Colocation', 'Turkcell', 'Istanbul', 'Turkey'),
    ('00000000-0000-0000-0000-000000000001', 'Hetzner Cloud', 'CloudProvider', 'Hetzner', 'Falkenstein', 'Germany'),
    ('00000000-0000-0000-0000-000000000001', 'Azure West Europe', 'CloudProvider', 'Microsoft Azure', 'Amsterdam', 'Netherlands');
