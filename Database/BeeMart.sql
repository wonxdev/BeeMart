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
