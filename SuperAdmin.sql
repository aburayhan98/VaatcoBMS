
INSERT INTO Users (Name, Email, PasswordHash, Role, IsApproved, EmailConfirmed, CreatedAt) 
VALUES (
    'System SuperAdmin', 
    'superadmin@vaatco.com', 
    '$2a$11$GIfqLUBqINe1jA02sFq7F..Jb37tC1u4hKz4bQW2mJ4Kwv7EwI2S6', 
    4, -- This represents UserRole.SuperAdmin in your ENUM
    1, -- true for IsApproved
    1, -- true for EmailConfirmed
    GETUTCDATE()
);