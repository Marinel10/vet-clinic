-- SCRUM-30: Database Setup
-- Vet Clinic Management System
-- Microsoft SQL Server (run in SQL Server Management Studio)
-- Run once on a SQL Server instance where you have CREATE DATABASE permission.

IF DB_ID(N'VetClinic') IS NULL
    CREATE DATABASE VetClinic;
GO
USE VetClinic;
GO

CREATE TABLE dbo.users (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    full_name NVARCHAR(150) NOT NULL,
    email NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL,
    phone NVARCHAR(30) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_users_role CHECK (role IN ('owner', 'veterinarian'))
);
GO

CREATE TABLE dbo.pets (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    owner_id INT NOT NULL,
    name NVARCHAR(100) NOT NULL,
    species NVARCHAR(100) NOT NULL,
    breed NVARCHAR(100) NULL,
    birth_date DATE NULL,
    sex VARCHAR(10) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_pets_owner FOREIGN KEY (owner_id) REFERENCES dbo.users(id),
    CONSTRAINT CK_pets_sex CHECK (sex IN ('male', 'female', 'unknown'))
);
GO

CREATE TABLE dbo.appointments (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pet_id INT NOT NULL,
    veterinarian_id INT NOT NULL,
    starts_at DATETIME2 NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'scheduled',
    reason NVARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_appointments_pet FOREIGN KEY (pet_id) REFERENCES dbo.pets(id),
    CONSTRAINT FK_appointments_vet FOREIGN KEY (veterinarian_id) REFERENCES dbo.users(id),
    CONSTRAINT CK_appointments_status CHECK (status IN ('scheduled', 'completed', 'cancelled'))
);
GO
-- Prevent two active appointments for one veterinarian at the same start time.
-- Overlapping appointments of different durations need additional application validation.
CREATE UNIQUE INDEX UX_appointments_vet_slot
    ON dbo.appointments(veterinarian_id, starts_at)
    WHERE status = 'scheduled';
CREATE INDEX IX_appointments_pet ON dbo.appointments(pet_id);
GO

CREATE TABLE dbo.medical_records (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pet_id INT NOT NULL,
    veterinarian_id INT NOT NULL,
    appointment_id INT NULL,
    diagnosis NVARCHAR(1000) NULL,
    treatment NVARCHAR(1000) NULL,
    notes NVARCHAR(MAX) NULL,
    recorded_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_medical_records_pet FOREIGN KEY (pet_id) REFERENCES dbo.pets(id),
    CONSTRAINT FK_medical_records_vet FOREIGN KEY (veterinarian_id) REFERENCES dbo.users(id),
    CONSTRAINT FK_medical_records_appointment FOREIGN KEY (appointment_id) REFERENCES dbo.appointments(id)
);
GO
CREATE INDEX IX_medical_records_pet ON dbo.medical_records(pet_id);
GO

CREATE TABLE dbo.vaccinations (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pet_id INT NOT NULL,
    veterinarian_id INT NULL,
    vaccine_name NVARCHAR(200) NOT NULL,
    administered_on DATE NOT NULL,
    next_due_on DATE NULL,
    notes NVARCHAR(500) NULL,
    CONSTRAINT FK_vaccinations_pet FOREIGN KEY (pet_id) REFERENCES dbo.pets(id),
    CONSTRAINT FK_vaccinations_vet FOREIGN KEY (veterinarian_id) REFERENCES dbo.users(id),
    CONSTRAINT CK_vaccinations_next_due CHECK (next_due_on IS NULL OR next_due_on >= administered_on)
);
GO
CREATE INDEX IX_vaccinations_pet ON dbo.vaccinations(pet_id);
GO

CREATE TABLE dbo.notifications (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    user_id INT NOT NULL,
    pet_id INT NULL,
    appointment_id INT NULL,
    type VARCHAR(40) NOT NULL,
    message NVARCHAR(1000) NOT NULL,
    scheduled_for DATETIME2 NULL,
    sent_at DATETIME2 NULL,
    is_read BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_notifications_user FOREIGN KEY (user_id) REFERENCES dbo.users(id),
    CONSTRAINT FK_notifications_pet FOREIGN KEY (pet_id) REFERENCES dbo.pets(id),
    CONSTRAINT FK_notifications_appointment FOREIGN KEY (appointment_id) REFERENCES dbo.appointments(id),
    CONSTRAINT CK_notifications_type CHECK (type IN (
        'appointment_reminder', 'vaccination_reminder',
        'appointment_change', 'veterinarian_booking'
    ))
);
GO
CREATE INDEX IX_notifications_user ON dbo.notifications(user_id);
GO

-- Check that all six tables were created.
SELECT name FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') ORDER BY name;
GO
