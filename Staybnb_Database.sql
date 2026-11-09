IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [Amenities] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [IconClass] nvarchar(50) NULL,
        CONSTRAINT [PK_Amenities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [ProfilePictureUrl] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [Action] nvarchar(200) NOT NULL,
        [ActivityType] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [HostProperties] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PricePerNight] decimal(18,2) NOT NULL,
        [CleaningFee] decimal(18,2) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [PropertyType] nvarchar(max) NOT NULL,
        [MaxGuests] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [HostId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_HostProperties] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HostProperties_AspNetUsers_HostId] FOREIGN KEY ([HostId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [Messages] (
        [Id] int NOT NULL IDENTITY,
        [SenderId] nvarchar(450) NOT NULL,
        [ReceiverId] nvarchar(450) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_AspNetUsers_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Messages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(100) NOT NULL,
        [Body] nvarchar(max) NULL,
        [Type] int NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [AmenityHostProperty] (
        [AmenitiesId] int NOT NULL,
        [PropertiesId] int NOT NULL,
        CONSTRAINT [PK_AmenityHostProperty] PRIMARY KEY ([AmenitiesId], [PropertiesId]),
        CONSTRAINT [FK_AmenityHostProperty_Amenities_AmenitiesId] FOREIGN KEY ([AmenitiesId]) REFERENCES [Amenities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AmenityHostProperty_HostProperties_PropertiesId] FOREIGN KEY ([PropertiesId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [Id] int NOT NULL IDENTITY,
        [PropertyId] int NOT NULL,
        [GuestId] nvarchar(450) NOT NULL,
        [CheckInDate] datetime2 NOT NULL,
        [CheckOutDate] datetime2 NOT NULL,
        [NumberOfGuests] int NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bookings_AspNetUsers_GuestId] FOREIGN KEY ([GuestId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_HostProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [CheckInProcesses] (
        [Id] int NOT NULL IDENTITY,
        [PropertyId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [StepsJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CheckInProcesses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CheckInProcesses_HostProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [HostApplications] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationUserId] nvarchar(450) NOT NULL,
        [PropertyId] int NOT NULL,
        [Status] int NOT NULL,
        [AppliedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_HostApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HostApplications_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HostApplications_HostProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [PropertyImages] (
        [Id] int NOT NULL IDENTITY,
        [PropertyId] int NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PropertyImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PropertyImages_HostProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [Reviews] (
        [Id] int NOT NULL IDENTITY,
        [PropertyId] int NOT NULL,
        [ReviewerId] nvarchar(450) NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reviews_AspNetUsers_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reviews_HostProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [WishlistItems] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [PropertyId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WishlistItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WishlistItems_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WishlistItems_HostProperties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [HostProperties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [GuestCheckIns] (
        [Id] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [CheckInProcessId] int NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_GuestCheckIns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GuestCheckIns_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GuestCheckIns_CheckInProcesses_CheckInProcessId] FOREIGN KEY ([CheckInProcessId]) REFERENCES [CheckInProcesses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE TABLE [GuestDocuments] (
        [Id] int NOT NULL IDENTITY,
        [GuestCheckInId] int NOT NULL,
        [DocumentType] nvarchar(100) NOT NULL,
        [FileName] nvarchar(200) NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_GuestDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GuestDocuments_GuestCheckIns_GuestCheckInId] FOREIGN KEY ([GuestCheckInId]) REFERENCES [GuestCheckIns] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_UserId] ON [ActivityLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AmenityHostProperty_PropertiesId] ON [AmenityHostProperty] ([PropertiesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_GuestId] ON [Bookings] ([GuestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_PropertyId] ON [Bookings] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CheckInProcesses_PropertyId] ON [CheckInProcesses] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GuestCheckIns_BookingId] ON [GuestCheckIns] ([BookingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GuestCheckIns_CheckInProcessId] ON [GuestCheckIns] ([CheckInProcessId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GuestDocuments_GuestCheckInId] ON [GuestDocuments] ([GuestCheckInId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HostApplications_ApplicationUserId] ON [HostApplications] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HostApplications_PropertyId] ON [HostApplications] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HostProperties_HostId] ON [HostProperties] ([HostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_ReceiverId] ON [Messages] ([ReceiverId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_SenderId] ON [Messages] ([SenderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PropertyImages_PropertyId] ON [PropertyImages] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_PropertyId] ON [Reviews] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_ReviewerId] ON [Reviews] ([ReviewerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WishlistItems_PropertyId] ON [WishlistItems] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WishlistItems_UserId] ON [WishlistItems] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028180022_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028180022_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101063221_AddPropertyRooms'
)
BEGIN
    ALTER TABLE [HostProperties] ADD [Bathrooms] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101063221_AddPropertyRooms'
)
BEGIN
    ALTER TABLE [HostProperties] ADD [Bedrooms] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101063221_AddPropertyRooms'
)
BEGIN
    ALTER TABLE [HostProperties] ADD [Beds] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101063221_AddPropertyRooms'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101063221_AddPropertyRooms', N'10.0.14');
END;

COMMIT;
GO

