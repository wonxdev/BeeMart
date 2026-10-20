/* ============================================================================
 * Project   : Binus Point of Sale (POS) Web App — BeeMart
 * File      : Database/BeeMart.sql
 * Purpose   : SQL Server Database Schema (DDL) for BeeMart POS System
 * Scope     : Phase 2: DDL only, seed data comes in Phase 3
 * Target    : Microsoft SQL Server (T-SQL) — Re-runnable / Idempotent
 * ============================================================================ */

SET NOCOUNT ON;
GO

/* ============================================================================
 * 0. DATABASE CREATION
 * ============================================================================ */
IF DB_ID(N'BeeMart') IS NULL
BEGIN
    CREATE DATABASE [BeeMart];
END
GO

USE [BeeMart];
GO

/* ============================================================================
 * 1. TABLE: LtPaymentMethod (DB-ENTITY-002)
 * ============================================================================ */
IF OBJECT_ID(N'dbo.LtPaymentMethod', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LtPaymentMethod]
    (
        [IdPaymentMethod] INT           IDENTITY(1,1) NOT NULL,
        [MethodName]      NVARCHAR(50)  NOT NULL,
        [Stsrc]           BIT           NOT NULL CONSTRAINT [DF_LtPaymentMethod_Stsrc]  DEFAULT (1),
        [DateIn]          DATETIME2     NOT NULL CONSTRAINT [DF_LtPaymentMethod_DateIn] DEFAULT (SYSDATETIME()),
        [UserIn]          NVARCHAR(36)  NULL     CONSTRAINT [DF_LtPaymentMethod_UserIn] DEFAULT (N'SYSTEM'),
        [Dateup]          DATETIME2     NULL,
        [Userup]          NVARCHAR(36)  NULL,

        CONSTRAINT [PK_LtPaymentMethod]            PRIMARY KEY CLUSTERED ([IdPaymentMethod] ASC),
        CONSTRAINT [UQ_LtPaymentMethod_MethodName] UNIQUE NONCLUSTERED ([MethodName] ASC),
        CONSTRAINT [CK_LtPaymentMethod_MethodName] CHECK ([MethodName] IN (N'Cash', N'Debit', N'Credit'))
    );
END
GO

/* ============================================================================
 * 2. TABLE: MsUser (DB-ENTITY-001)
 * ============================================================================ */
IF OBJECT_ID(N'dbo.MsUser', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MsUser]
    (
        [IdUser]       NVARCHAR(5)   NOT NULL,
        [Username]     NVARCHAR(50)  NOT NULL,
        [FullName]     NVARCHAR(300) NOT NULL,
        [PasswordHash] NVARCHAR(255) NOT NULL,
        [Role]         NVARCHAR(20)  NOT NULL,
        [Stsrc]        BIT           NOT NULL CONSTRAINT [DF_MsUser_Stsrc]  DEFAULT (1),
        [DateIn]       DATETIME2     NOT NULL CONSTRAINT [DF_MsUser_DateIn] DEFAULT (SYSDATETIME()),
        [UserIn]       NVARCHAR(36)  NULL     CONSTRAINT [DF_MsUser_UserIn] DEFAULT (N'SYSTEM'),
        [Dateup]       DATETIME2     NULL,
        [Userup]       NVARCHAR(36)  NULL,

        CONSTRAINT [PK_MsUser]        PRIMARY KEY CLUSTERED ([IdUser] ASC),
        CONSTRAINT [CK_MsUser_Role]   CHECK ([Role] IN (N'Admin', N'Cashier'))
    );
END
GO

/* ============================================================================
 * 3. TABLE: MsProduct (DB-ENTITY-003)
 * ============================================================================ */
IF OBJECT_ID(N'dbo.MsProduct', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MsProduct]
    (
        [IdProduct] NVARCHAR(36)  NOT NULL CONSTRAINT [DF_MsProduct_IdProduct] DEFAULT (CONVERT(NVARCHAR(36), NEWID())),
        [Name]      NVARCHAR(100) NOT NULL,
        [Weight]    DECIMAL(10,2) NOT NULL CONSTRAINT [DF_MsProduct_Weight]    DEFAULT (0),
        [Stock]     INT           NOT NULL CONSTRAINT [DF_MsProduct_Stock]     DEFAULT (0),
        [Price]     DECIMAL(18,2) NOT NULL CONSTRAINT [DF_MsProduct_Price]     DEFAULT (0),
        [Discount]  DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_MsProduct_Discount]  DEFAULT (0),
        [Stsrc]     BIT           NOT NULL CONSTRAINT [DF_MsProduct_Stsrc]     DEFAULT (1),
        [DateIn]    DATETIME2     NOT NULL CONSTRAINT [DF_MsProduct_DateIn]    DEFAULT (SYSDATETIME()),
        [UserIn]    NVARCHAR(36)  NULL     CONSTRAINT [DF_MsProduct_UserIn]    DEFAULT (N'SYSTEM'),
        [Dateup]    DATETIME2     NULL,
        [Userup]    NVARCHAR(36)  NULL,

        CONSTRAINT [PK_MsProduct] PRIMARY KEY CLUSTERED ([IdProduct] ASC)
    );
END
GO

/* ============================================================================
 * 4. TABLE: TrTransaction (DB-ENTITY-004)
 * ============================================================================ */
IF OBJECT_ID(N'dbo.TrTransaction', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TrTransaction]
    (
        [IdTransaction]   NVARCHAR(36)  NOT NULL CONSTRAINT [DF_TrTransaction_IdTransaction]   DEFAULT (CONVERT(NVARCHAR(36), NEWID())),
        [InvoiceNo]       NVARCHAR(20)  NOT NULL,
        [TransactionDate] DATETIME2     NOT NULL CONSTRAINT [DF_TrTransaction_TransactionDate] DEFAULT (SYSDATETIME()),
        [IdUser]          NVARCHAR(5)   NOT NULL,
        [IdPaymentMethod] INT           NOT NULL,
        [TotalPrice]      DECIMAL(18,2) NOT NULL CONSTRAINT [DF_TrTransaction_TotalPrice]      DEFAULT (0),
        [Stsrc]           BIT           NOT NULL CONSTRAINT [DF_TrTransaction_Stsrc]           DEFAULT (1),
        [DateIn]          DATETIME2     NOT NULL CONSTRAINT [DF_TrTransaction_DateIn]          DEFAULT (SYSDATETIME()),
        [UserIn]          NVARCHAR(36)  NULL,
        [Dateup]          DATETIME2     NULL,
        [Userup]          NVARCHAR(36)  NULL,

        CONSTRAINT [PK_TrTransaction]                 PRIMARY KEY CLUSTERED ([IdTransaction] ASC),
        CONSTRAINT [FK_TrTransaction_MsUser]          FOREIGN KEY ([IdUser])          REFERENCES [dbo].[MsUser] ([IdUser]),
        CONSTRAINT [FK_TrTransaction_LtPaymentMethod] FOREIGN KEY ([IdPaymentMethod]) REFERENCES [dbo].[LtPaymentMethod] ([IdPaymentMethod])
    );
END
GO

/* ============================================================================
 * 5. TABLE: TrTransactionDetail (DB-ENTITY-005)
 * ============================================================================ */
IF OBJECT_ID(N'dbo.TrTransactionDetail', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TrTransactionDetail]
    (
        [IdTransactionDetail] NVARCHAR(36)  NOT NULL CONSTRAINT [DF_TrTransactionDetail_IdTransactionDetail] DEFAULT (CONVERT(NVARCHAR(36), NEWID())),
        [IdTransaction]       NVARCHAR(36)  NOT NULL,
        [IdProduct]           NVARCHAR(36)  NOT NULL,
        [Qty]                 INT           NOT NULL CONSTRAINT [DF_TrTransactionDetail_Qty]                 DEFAULT (1),
        [Price]               DECIMAL(18,2) NOT NULL CONSTRAINT [DF_TrTransactionDetail_Price]               DEFAULT (0),
        [Discount]            DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_TrTransactionDetail_Discount]            DEFAULT (0),
        [SubTotal]            DECIMAL(18,2) NOT NULL CONSTRAINT [DF_TrTransactionDetail_SubTotal]            DEFAULT (0),
        [Stsrc]               BIT           NOT NULL CONSTRAINT [DF_TrTransactionDetail_Stsrc]               DEFAULT (1),
        [DateIn]              DATETIME2     NOT NULL CONSTRAINT [DF_TrTransactionDetail_DateIn]              DEFAULT (SYSDATETIME()),
        [UserIn]              NVARCHAR(36)  NULL,
        [Dateup]              DATETIME2     NULL,
        [Userup]              NVARCHAR(36)  NULL,

        CONSTRAINT [PK_TrTransactionDetail]               PRIMARY KEY CLUSTERED ([IdTransactionDetail] ASC),
        CONSTRAINT [FK_TrTransactionDetail_TrTransaction] FOREIGN KEY ([IdTransaction]) REFERENCES [dbo].[TrTransaction] ([IdTransaction]),
        CONSTRAINT [FK_TrTransactionDetail_MsProduct]     FOREIGN KEY ([IdProduct])     REFERENCES [dbo].[MsProduct] ([IdProduct])
    );
END
GO

/* ============================================================================
 * 6. SECTION 5.4 CONSTRAINTS & INDEXES (IDEMPOTENT GUARDS)
 * ============================================================================ */

-- 6.1 UQ_MsUser_Username: Unique index on MsUser(Username)
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'UQ_MsUser_Username'
      AND [object_id] = OBJECT_ID(N'dbo.MsUser')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_MsUser_Username]
        ON [dbo].[MsUser] ([Username] ASC);
END
GO

-- 6.2 UQ_TrTransaction_InvoiceNo: Unique index on TrTransaction(InvoiceNo)
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'UQ_TrTransaction_InvoiceNo'
      AND [object_id] = OBJECT_ID(N'dbo.TrTransaction')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TrTransaction_InvoiceNo]
        ON [dbo].[TrTransaction] ([InvoiceNo] ASC);
END
GO

-- 6.3 IX_TrTransaction_TransactionDate_IdUser: Index on TrTransaction(TransactionDate ASC, IdUser)
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_TrTransaction_TransactionDate_IdUser'
      AND [object_id] = OBJECT_ID(N'dbo.TrTransaction')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TrTransaction_TransactionDate_IdUser]
        ON [dbo].[TrTransaction] ([TransactionDate] ASC, [IdUser] ASC);
END
GO

-- 6.4 IX_MsProduct_Name: Index on MsProduct(Name)
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MsProduct_Name'
      AND [object_id] = OBJECT_ID(N'dbo.MsProduct')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MsProduct_Name]
        ON [dbo].[MsProduct] ([Name] ASC);
END
GO

-- PHASE 3: SEED DATA GOES BELOW THIS LINE

USE [BeeMart];
GO

/* ============================================================================
 * 7. SEED DATA: LtPaymentMethod (3 rows: Cash = 1, Debit = 2, Credit = 3)
 * ============================================================================ */
IF NOT EXISTS (SELECT 1 FROM [dbo].[LtPaymentMethod] WHERE [MethodName] = N'Cash')
BEGIN
    INSERT INTO [dbo].[LtPaymentMethod] ([MethodName], [UserIn])
    VALUES (N'Cash', N'SYSTEM');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[LtPaymentMethod] WHERE [MethodName] = N'Debit')
BEGIN
    INSERT INTO [dbo].[LtPaymentMethod] ([MethodName], [UserIn])
    VALUES (N'Debit', N'SYSTEM');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[LtPaymentMethod] WHERE [MethodName] = N'Credit')
BEGIN
    INSERT INTO [dbo].[LtPaymentMethod] ([MethodName], [UserIn])
    VALUES (N'Credit', N'SYSTEM');
END
GO

/* ============================================================================
 * 8. SEED DATA: MsUser (6 rows — PRD Section 5.4, BCrypt work factor 11)
 * ============================================================================ */
INSERT INTO [dbo].[MsUser] ([IdUser], [Username], [FullName], [PasswordHash], [Role], [UserIn])
SELECT v.[IdUser], v.[Username], v.[FullName], v.[PasswordHash], v.[Role], N'SYSTEM'
FROM (VALUES
    (N'00001', N'admin',  N'Admin',  N'$2a$11$pN912e6CZDcorrW.tWWgEuZZ62n0NuFBYyHmDLVf/Dq8B2ZpQP0gS', N'Admin'),
    (N'76985', N'steven', N'Steven', N'$2a$11$U/upEfJGIJuE869ojcW9geEv5DNZzUgHBgPUYlkLPz2sJ49e7R7Ku', N'Cashier'),
    (N'66859', N'gabri',  N'Gabri',  N'$2a$11$zIOcPAtRmprGh0qhHshkD.nSOmOnhWmCUSYmiikfiMgPFiZnHxPIa', N'Cashier'),
    (N'55794', N'mona',   N'Mona',   N'$2a$11$.RKosWyuHOW/9iaTQc.Pf..0Kwwra7IqEh3tdlmQ/W9rpetLpeYfO', N'Cashier'),
    (N'78886', N'felix',  N'Felix',  N'$2a$11$eh8dS/QuUvDd.hAtB6XHyunpHAJvDTltu4njOlddBFcmv2tkF1yeK', N'Cashier'),
    (N'55356', N'gobuta', N'Gobuta', N'$2a$11$/VtQ/Bs.gOHv6ZZ8H6B0J.1GF96hRw.fpWniZbQcge0yhYuMkJLSq', N'Cashier')
) AS v ([IdUser], [Username], [FullName], [PasswordHash], [Role])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[MsUser] AS u
    WHERE u.[Username] = v.[Username]
);
GO

/* ============================================================================
 * 9. SEED DATA: MsProduct (22 rows: 12 Apel, 4 Susu, 6 General Groceries)
 * ============================================================================ */
INSERT INTO [dbo].[MsProduct] ([IdProduct], [Name], [Weight], [Stock], [Price], [Discount], [UserIn])
SELECT v.[IdProduct], v.[Name], v.[Weight], v.[Stock], v.[Price], v.[Discount], N'SYSTEM'
FROM (VALUES
    -- Core PRD products (0001 - 0004)
    (N'a1b2c3d4-0001-4000-8000-000000000001', N'Apel Fuji',                  CAST(1.00 AS DECIMAL(10,2)),  50, CAST(35000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0002-4000-8000-000000000002', N'Apel Merah',                 CAST(1.00 AS DECIMAL(10,2)),  30, CAST(30000.00 AS DECIMAL(18,2)), CAST( 5.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0003-4000-8000-000000000003', N'Apel Hijau',                 CAST(1.00 AS DECIMAL(10,2)),  25, CAST(28000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0004-4000-8000-000000000004', N'Susu UHT Full Cream',        CAST(1.00 AS DECIMAL(10,2)),  40, CAST(18000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),

    -- Additional Apel products (0005 - 0013) -> Total 12 'Apel' products to test >10 search limit
    (N'a1b2c3d4-0005-4000-8000-000000000005', N'Apel Malang',                CAST(1.00 AS DECIMAL(10,2)),  60, CAST(24000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0006-4000-8000-000000000006', N'Apel Royal Gala',            CAST(1.00 AS DECIMAL(10,2)),  35, CAST(38000.00 AS DECIMAL(18,2)), CAST( 5.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0007-4000-8000-000000000007', N'Apel Granny Smith',          CAST(1.00 AS DECIMAL(10,2)),  20, CAST(42000.00 AS DECIMAL(18,2)), CAST(10.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0008-4000-8000-000000000008', N'Apel Envy',                  CAST(1.00 AS DECIMAL(10,2)),  25, CAST(55000.00 AS DECIMAL(18,2)), CAST( 5.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0009-4000-8000-000000000009', N'Apel Pacific Rose',          CAST(1.00 AS DECIMAL(10,2)),  30, CAST(48000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0010-4000-8000-000000000010', N'Apel Honeycrisp',            CAST(1.00 AS DECIMAL(10,2)),  22, CAST(52000.00 AS DECIMAL(18,2)), CAST(10.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0011-4000-8000-000000000011', N'Apel Pink Lady',             CAST(1.00 AS DECIMAL(10,2)),  28, CAST(45000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0012-4000-8000-000000000012', N'Apel Red Delicious',         CAST(1.00 AS DECIMAL(10,2)),  45, CAST(32000.00 AS DECIMAL(18,2)), CAST( 5.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0013-4000-8000-000000000013', N'Apel Manalagi',              CAST(1.00 AS DECIMAL(10,2)),  55, CAST(26000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),

    -- Additional Susu products (0014 - 0016) -> Total 4 'Susu' products
    (N'a1b2c3d4-0014-4000-8000-000000000014', N'Susu UHT Cokelat',           CAST(1.00 AS DECIMAL(10,2)),  45, CAST(18500.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0015-4000-8000-000000000015', N'Susu UHT Stroberi',          CAST(1.00 AS DECIMAL(10,2)),  35, CAST(18500.00 AS DECIMAL(18,2)), CAST( 5.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0016-4000-8000-000000000016', N'Susu Kental Manis Putih',    CAST(0.37 AS DECIMAL(10,2)),  60, CAST(12500.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),

    -- Other grocery products (0017 - 0022) -> 6 unrelated products
    (N'a1b2c3d4-0017-4000-8000-000000000017', N'Beras Pandan Wangi 5 Kg',    CAST(5.00 AS DECIMAL(10,2)),  40, CAST(75000.00 AS DECIMAL(18,2)), CAST( 5.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0018-4000-8000-000000000018', N'Minyak Goreng Sawit 2 L',    CAST(1.80 AS DECIMAL(10,2)),  50, CAST(36000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0019-4000-8000-000000000019', N'Gula Pasir Putih 1 Kg',      CAST(1.00 AS DECIMAL(10,2)),  70, CAST(17500.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0020-4000-8000-000000000020', N'Mie Instan Goreng',          CAST(0.09 AS DECIMAL(10,2)), 100, CAST( 3500.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0021-4000-8000-000000000021', N'Telur Ayam Negeri 1 Kg',     CAST(1.00 AS DECIMAL(10,2)),  30, CAST(29000.00 AS DECIMAL(18,2)), CAST( 0.00 AS DECIMAL(5,2))),
    (N'a1b2c3d4-0022-4000-8000-000000000022', N'Kopi Bubuk Arabika 200 g',   CAST(0.20 AS DECIMAL(10,2)),  35, CAST(32000.00 AS DECIMAL(18,2)), CAST(10.00 AS DECIMAL(5,2)))
) AS v ([IdProduct], [Name], [Weight], [Stock], [Price], [Discount])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[MsProduct] AS p
    WHERE p.[IdProduct] = v.[IdProduct]
);
GO

