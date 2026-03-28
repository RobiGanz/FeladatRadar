-- ============================================================
-- FeladatRadar — Teljesítményteszt adatok
-- FUTTASD SSMS-BEN!
-- ============================================================

USE [FeladatRadar]
GO

-- ════════════════════════════════════════════
-- 0. Teszt felhasználók létrehozása (ha nincsenek)
-- ════════════════════════════════════════════
-- BCrypt hash = Teszt123!
DECLARE @Hash NVARCHAR(255) = '$2b$11$45R1Ux6RLSEWcsFRep.k2.B97YlOuYjROJI6YuYkCK1fIq87LG2RG';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'testdiak')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, UserRole, IsActive)
    VALUES ('testdiak', @Hash, 'testdiak@example.hu', 'Teszt', 'Diák', 'Student', 1);
    PRINT 'testdiak felhasználó létrehozva.';
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'tesztanar')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, UserRole, IsActive)
    VALUES ('tesztanar', @Hash, 'tesztanar@example.hu', 'Teszt', 'Tanár', 'Teacher', 1);
    PRINT 'tesztanar felhasználó létrehozva.';
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'tesztadmin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, UserRole, IsActive)
    VALUES ('tesztadmin', @Hash, 'tesztadmin@example.hu', 'Teszt', 'Admin', 'Admin', 1);
    PRINT 'tesztadmin felhasználó létrehozva.';
END

-- 18 extra diák a csoportteszt adatokhoz
DECLARE @i INT = 1;
WHILE @i <= 18
BEGIN
    DECLARE @uname NVARCHAR(50) = 'tesztdiak' + CAST(@i AS NVARCHAR(5));
    DECLARE @email NVARCHAR(100) = 'tesztdiak' + CAST(@i AS NVARCHAR(5)) + '@example.hu';
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @uname)
    BEGIN
        INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, UserRole, IsActive)
        VALUES (@uname, @Hash, @email, 'Diák', CAST(@i AS NVARCHAR(5)), 'Student', 1);
    END
    SET @i = @i + 1;
END
PRINT '18 extra tesztdiák létrehozva.';

-- ════════════════════════════════════════════
-- 1. Tantárgyak (ha nincsenek)
-- ════════════════════════════════════════════
DECLARE @subjects TABLE (Name NVARCHAR(100), Code NVARCHAR(20));
INSERT INTO @subjects VALUES
('Matematika','MAT01'),('Fizika','FIZ01'),('Informatika','INF01'),
('Történelem','TORT01'),('Angol nyelv','ANG01'),('Biológia','BIO01'),
('Kémia','KEM01'),('Földrajz','FOL01'),('Irodalom','IRO01'),('Testnevelés','TES01');

INSERT INTO Subjects (SubjectName, SubjectCode, IsActive, CurrentEnrollment, IsPrivate)
SELECT s.Name, s.Code, 1, 0, 0
FROM @subjects s
WHERE NOT EXISTS (SELECT 1 FROM Subjects WHERE SubjectCode = s.Code);
PRINT 'Tantárgyak ellenőrizve/létrehozva.';

-- Diák beiratkoztatása az összes tantárgyra
DECLARE @StudentID INT = (SELECT TOP 1 UserID FROM Users WHERE Username = 'testdiak');
DECLARE @TeacherID INT = (SELECT TOP 1 UserID FROM Users WHERE Username = 'tesztanar');

INSERT INTO StudentSubjects (StudentID, SubjectID)
SELECT @StudentID, SubjectID FROM Subjects
WHERE SubjectID NOT IN (SELECT SubjectID FROM StudentSubjects WHERE StudentID = @StudentID);
PRINT 'Diák beiratkoztatva.';

-- ════════════════════════════════════════════
-- 2. 50 FELADAT (sp_GetMyTasks teszt)
-- ════════════════════════════════════════════
DECLARE @taskCount INT = (SELECT COUNT(*) FROM Tasks WHERE CreatedBy = @StudentID);
IF @taskCount < 50
BEGIN
    DECLARE @t INT = @taskCount + 1;
    DECLARE @subjectIDs TABLE (ID INT, RowNum INT IDENTITY(1,1));
    INSERT INTO @subjectIDs (ID) SELECT SubjectID FROM Subjects WHERE IsActive = 1;
    DECLARE @subCount INT = (SELECT COUNT(*) FROM @subjectIDs);

    WHILE @t <= 50
    BEGIN
        DECLARE @sID INT = (SELECT ID FROM @subjectIDs WHERE RowNum = ((@t % @subCount) + 1));
        DECLARE @taskType NVARCHAR(50) = CASE @t % 5
            WHEN 0 THEN 'Assignment'
            WHEN 1 THEN 'Homework'
            WHEN 2 THEN 'Exam'
            WHEN 3 THEN 'Project'
            WHEN 4 THEN 'Personal'
        END;
        DECLARE @dueDate DATETIME = DATEADD(DAY, @t - 25, GETDATE());
        DECLARE @completed BIT = CASE WHEN @t <= 15 THEN 1 ELSE 0 END;

        INSERT INTO Tasks (CreatedBy, SubjectID, Title, Description, DueDate, TaskType, IsCompleted, RecurrenceType)
        VALUES (@StudentID, @sID,
            'Tesztfeladat #' + CAST(@t AS NVARCHAR(5)),
            'Automatikusan generált tesztfeladat a teljesítményteszthez.',
            @dueDate, @taskType, @completed, 'None');

        SET @t = @t + 1;
    END
    PRINT '50 feladat létrehozva.';
END
ELSE
    PRINT 'Már van 50+ feladat.';

-- ════════════════════════════════════════════
-- 3. CSOPORT + 20 TAG (sp_GetGroupMembers teszt)
-- ════════════════════════════════════════════
DECLARE @TestGroupID INT;
SELECT @TestGroupID = GroupID FROM Groups WHERE GroupName = 'Teljesítményteszt csoport';

IF @TestGroupID IS NULL
BEGIN
    INSERT INTO Groups (GroupName, CreatedBy) VALUES ('Teljesítményteszt csoport', @StudentID);
    SET @TestGroupID = SCOPE_IDENTITY();

    -- Létrehozó automatikusan tag
    INSERT INTO GroupMembers (GroupID, StudentID) VALUES (@TestGroupID, @StudentID);

    -- Tanár hozzáadása
    IF NOT EXISTS (SELECT 1 FROM GroupMembers WHERE GroupID = @TestGroupID AND StudentID = @TeacherID)
        INSERT INTO GroupMembers (GroupID, StudentID) VALUES (@TestGroupID, @TeacherID);

    -- 18 extra diák hozzáadása (összesen 20 tag)
    DECLARE @m INT = 1;
    WHILE @m <= 18
    BEGIN
        DECLARE @memberID INT = (SELECT UserID FROM Users WHERE Username = 'tesztdiak' + CAST(@m AS NVARCHAR(5)));
        IF @memberID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM GroupMembers WHERE GroupID = @TestGroupID AND StudentID = @memberID)
            INSERT INTO GroupMembers (GroupID, StudentID) VALUES (@TestGroupID, @memberID);
        SET @m = @m + 1;
    END
    PRINT '20 tagú csoport létrehozva (GroupID=' + CAST(@TestGroupID AS VARCHAR(10)) + ').';
END
ELSE
    PRINT 'Teljesítményteszt csoport már létezik (GroupID=' + CAST(@TestGroupID AS VARCHAR(10)) + ').';

-- ════════════════════════════════════════════
-- 4. 60 ÓRAREND BEJEGYZÉS (heti nézet teszt)
-- ════════════════════════════════════════════
DECLARE @schedCount INT = (SELECT COUNT(*) FROM ScheduleEntries WHERE StudentID = @StudentID);
IF @schedCount < 60
BEGIN
    DELETE FROM ScheduleEntries WHERE StudentID = @StudentID;

    DECLARE @s INT = 0;
    WHILE @s < 60
    BEGIN
        DECLARE @dayOfWeek INT = (@s % 5) + 1;
        DECLARE @slotIndex INT = @s / 5;
        DECLARE @hour INT = 7 + @slotIndex;
        DECLARE @subjID INT = (SELECT ID FROM @subjectIDs WHERE RowNum = ((@s % @subCount) + 1));

        IF @hour < 19
        BEGIN
            DECLARE @startTime TIME = CAST(CAST(@hour AS VARCHAR(2)) + ':00' AS TIME);
            DECLARE @endTime TIME = CAST(CAST(@hour AS VARCHAR(2)) + ':45' AS TIME);

            INSERT INTO ScheduleEntries (StudentID, SubjectID, DayOfWeek, StartTime, EndTime, Location, RecurrenceType, CreatedAt)
            VALUES (@StudentID, @subjID, @dayOfWeek, @startTime, @endTime,
                'Terem ' + CAST(100 + (@s % 20) AS VARCHAR(5)),
                'Weekly', GETDATE());
        END
        SET @s = @s + 1;
    END
    PRINT '60 órarend bejegyzés létrehozva.';
END
ELSE
    PRINT 'Már van 60+ órarend bejegyzés.';

-- ════════════════════════════════════════════
-- 5. 10 SZAVAZÁS + 40 OPCIÓ (csoportoldal teszt)
-- ════════════════════════════════════════════
DECLARE @pollCount INT = (SELECT COUNT(*) FROM Polls WHERE GroupID = @TestGroupID);
IF @pollCount < 10
BEGIN
    DECLARE @p INT = @pollCount + 1;
    WHILE @p <= 10
    BEGIN
        DECLARE @pollID INT;

        INSERT INTO Polls (GroupID, CreatedBy, Question, IsActive)
        VALUES (@TestGroupID, @StudentID,
            'Tesztszavazás #' + CAST(@p AS NVARCHAR(5)) + ': Melyik opciót választod?', 1);
        SET @pollID = SCOPE_IDENTITY();

        INSERT INTO PollOptions (PollID, OptionText, SortOrder) VALUES (@pollID, 'A opció', 1);
        INSERT INTO PollOptions (PollID, OptionText, SortOrder) VALUES (@pollID, 'B opció', 2);
        INSERT INTO PollOptions (PollID, OptionText, SortOrder) VALUES (@pollID, 'C opció', 3);
        INSERT INTO PollOptions (PollID, OptionText, SortOrder) VALUES (@pollID, 'D opció', 4);

        SET @p = @p + 1;
    END
    PRINT '10 szavazás + 40 opció létrehozva.';
END
ELSE
    PRINT 'Már van 10+ szavazás.';

-- ════════════════════════════════════════════
-- 6. ELLENŐRZÉS — Végeredmény
-- ════════════════════════════════════════════
PRINT '';
PRINT '══════════════════════════════════════';
PRINT '  TELJESÍTMÉNYTESZT ADATOK ÖSSZEGZÉS';
PRINT '══════════════════════════════════════';

SELECT 'Feladatok (Tasks)' AS Kategória,
       COUNT(*) AS Darab,
       CASE WHEN COUNT(*) >= 50 THEN '✓ OK' ELSE '✗ HIÁNYZIK' END AS Státusz
FROM Tasks WHERE CreatedBy = @StudentID
UNION ALL
SELECT 'Csoporttagok (GroupMembers)',
       COUNT(*),
       CASE WHEN COUNT(*) >= 20 THEN '✓ OK' ELSE '✗ HIÁNYZIK' END
FROM GroupMembers WHERE GroupID = @TestGroupID
UNION ALL
SELECT 'Órarend (ScheduleEntries)',
       COUNT(*),
       CASE WHEN COUNT(*) >= 60 THEN '✓ OK' ELSE '✗ HIÁNYZIK' END
FROM ScheduleEntries WHERE StudentID = @StudentID
UNION ALL
SELECT 'Szavazások (Polls)',
       COUNT(*),
       CASE WHEN COUNT(*) >= 10 THEN '✓ OK' ELSE '✗ HIÁNYZIK' END
FROM Polls WHERE GroupID = @TestGroupID
UNION ALL
SELECT 'Szavazási opciók (PollOptions)',
       (SELECT COUNT(*) FROM PollOptions po
        INNER JOIN Polls p ON p.PollID = po.PollID
        WHERE p.GroupID = @TestGroupID),
       CASE WHEN (SELECT COUNT(*) FROM PollOptions po
        INNER JOIN Polls p ON p.PollID = po.PollID
        WHERE p.GroupID = @TestGroupID) >= 40 THEN '✓ OK' ELSE '✗ HIÁNYZIK' END;

-- ════════════════════════════════════════════
-- 7. TELJESÍTMÉNYMÉRÉS — SP válaszidők
-- ════════════════════════════════════════════
PRINT '';
PRINT '══════════════════════════════════════';
PRINT '  SP VÁLASZIDŐ MÉRÉS';
PRINT '══════════════════════════════════════';

DECLARE @start DATETIME2, @end DATETIME2, @ms INT;

SET @start = SYSDATETIME();
EXEC sp_GetMyTasks @StudentID = @StudentID;
SET @end = SYSDATETIME();
SET @ms = DATEDIFF(MICROSECOND, @start, @end) / 1000;
PRINT 'sp_GetMyTasks:        ' + CAST(@ms AS VARCHAR(10)) + ' ms (elvárás: < 100 ms)';

SET @start = SYSDATETIME();
EXEC sp_GetGroupMembers @GroupID = @TestGroupID, @StudentID = @StudentID;
SET @end = SYSDATETIME();
SET @ms = DATEDIFF(MICROSECOND, @start, @end) / 1000;
PRINT 'sp_GetGroupMembers:   ' + CAST(@ms AS VARCHAR(10)) + ' ms (elvárás: < 50 ms)';

SET @start = SYSDATETIME();
EXEC sp_GetMySchedule @StudentID = @StudentID;
SET @end = SYSDATETIME();
SET @ms = DATEDIFF(MICROSECOND, @start, @end) / 1000;
PRINT 'sp_GetMySchedule:     ' + CAST(@ms AS VARCHAR(10)) + ' ms (elvárás: < 200 ms)';

SET @start = SYSDATETIME();
EXEC sp_GetGroupPolls @GroupID = @TestGroupID, @UserID = @StudentID;
SET @end = SYSDATETIME();
SET @ms = DATEDIFF(MICROSECOND, @start, @end) / 1000;
PRINT 'sp_GetGroupPolls:     ' + CAST(@ms AS VARCHAR(10)) + ' ms (elvárás: < 300 ms)';

PRINT '';
PRINT 'Teljesítményteszt kész!';
GO