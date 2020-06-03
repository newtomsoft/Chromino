SELECT avg_fragmentation_in_percent AS FragmentationInPercent,
OBJECT_SCHEMA_NAME (Stats.object_id) AS SchemaName, 
OBJECT_NAME (Stats.object_id) AS TableName,
Indexes.name
FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('dbo.T_CLIENT_CLI') , NULL, NULL , 'LIMITED') AS Stats
INNER JOIN sys.indexes AS Indexes ON Stats.object_id = Indexes.object_id AND Stats.index_id = Indexes.index_id
ORDER BY avg_fragmentation_in_percent DESC

