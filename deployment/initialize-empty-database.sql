-- Run against the selected, empty SQL Server database.
-- This script refuses to proceed if either target table already exists.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Candidates') IS NOT NULL OR OBJECT_ID(N'dbo.Votes') IS NOT NULL
        THROW 50000, 'dbo.Candidates or dbo.Votes already exists; initialization stopped.', 1;

    CREATE TABLE dbo.Candidates (
        Id       INT IDENTITY(1,1) PRIMARY KEY,
        Name     NVARCHAR(100) NOT NULL,
        Position NVARCHAR(30) NOT NULL,
        Grade    INT NULL,
        CONSTRAINT CK_Candidates_Position CHECK (Position IN
            ('President', 'VicePresident', 'Secretary', 'Treasurer', 'Auditor', 'PIO', 'PO', 'Representative')),
        CONSTRAINT CK_Candidates_Grade CHECK (
            (Position = 'Representative' AND Grade BETWEEN 8 AND 12)
            OR (Position <> 'Representative' AND Grade IS NULL))
    );

    CREATE TABLE dbo.Votes (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        VoterName         NVARCHAR(100) NOT NULL,
        VoterIdNo         NVARCHAR(30) NOT NULL,
        VoterGrade        INT NOT NULL CONSTRAINT CK_Votes_VoterGrade CHECK (VoterGrade BETWEEN 7 AND 11),
        PresidentId       INT NULL,
        VicePresidentId   INT NULL,
        SecretaryId       INT NULL,
        TreasurerId       INT NULL,
        AuditorId         INT NULL,
        PioId             INT NULL,
        PoId              INT NULL,
        RepresentativeId  INT NULL,
        Representative2Id INT NULL,
        VotedAt           DATETIME2 NOT NULL CONSTRAINT DF_Votes_VotedAt DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Votes_VoterIdNo UNIQUE (VoterIdNo),
        CONSTRAINT FK_Votes_President       FOREIGN KEY (PresidentId)       REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_VicePresident   FOREIGN KEY (VicePresidentId)   REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Secretary       FOREIGN KEY (SecretaryId)       REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Treasurer       FOREIGN KEY (TreasurerId)       REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Auditor         FOREIGN KEY (AuditorId)         REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Pio             FOREIGN KEY (PioId)             REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Po              FOREIGN KEY (PoId)              REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Representative  FOREIGN KEY (RepresentativeId)  REFERENCES dbo.Candidates(Id),
        CONSTRAINT FK_Votes_Representative2 FOREIGN KEY (Representative2Id) REFERENCES dbo.Candidates(Id)
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;


