-- From http://habrahabr.ru/post/209698/
DECLARE @SQL NVARCHAR(MAX)

DECLARE cur CURSOR LOCAL READ_ONLY FORWARD_ONLY FOR
    SELECT '
    ALTER INDEX [' + i.name + N'] ON [' + SCHEMA_NAME(o.[schema_id]) + '].[' + o.name + '] ' +
        CASE WHEN s.avg_fragmentation_in_percent > 30
            THEN 'REBUILD WITH (SORT_IN_TEMPDB = ON'
                -- Enterprise, Developer
                + CASE WHEN SERVERPROPERTY('EditionID') IN (1804890536, -2117995310)
                        THEN ', ONLINE = ON'
                        ELSE ''
                  END + ')'
            ELSE 'REORGANIZE'
        END + ';'
    FROM (
        SELECT 
              s.[object_id]
            , s.index_id
            , avg_fragmentation_in_percent = MAX(s.avg_fragmentation_in_percent)
        FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'DETAILED') s
        WHERE s.page_count > 128 -- > 1 MB
            AND s.index_id > 0 -- <> HEAP
            AND s.avg_fragmentation_in_percent > 5
        GROUP BY s.[object_id], s.index_id
    ) s
    JOIN sys.indexes i WITH(NOLOCK) ON s.[object_id] = i.[object_id] AND s.index_id = i.index_id
    JOIN sys.objects o WITH(NOLOCK) ON o.[object_id] = s.[object_id]

OPEN cur

FETCH NEXT FROM cur INTO @SQL

WHILE @@FETCH_STATUS = 0 BEGIN

    EXEC sys.sp_executesql @SQL

    FETCH NEXT FROM cur INTO @SQL
    
END 

CLOSE cur 
DEALLOCATE cur 