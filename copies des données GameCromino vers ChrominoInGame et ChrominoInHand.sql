--*******************************************************************
-- A appliquer après migration 20200202025951_createTables***********
-- !!! et OBLIGATOIREMENT avant suppression table GameChromino !!! **
--*******************************************************************

--copie des données de GameChromino dans ChrominoInGame
INSERT INTO ChrominoInGame(ChrominoId, GameId, XPosition, YPosition, Orientation)
SELECT ChrominoId, GameId, XPosition, YPosition, Orientation 
FROM GameChromino
where State = 3


--copie des données de GameChromino dans ChrominoInHand
INSERT INTO ChrominoInHand (ChrominoId, GameId, PlayerId, Position)
SELECT ChrominoId, GameId, PlayerId, 1  
FROM GameChromino
where State = 2

-- création positions différentes 
WHILE (select count(Id)
 from ChrominoInHand
 WHERE EXISTS (
  SELECT *
  FROM (SELECT * FROM ChrominoInHand) AS t2
  WHERE t2.id > ChrominoInHand.id AND t2.Position=ChrominoInHand.Position and t2.GameId = ChrominoInHand.GameId and t2.PlayerId = ChrominoInHand.PlayerId)
) > 0
BEGIN
UPDATE ChrominoInHand
   SET ChrominoInHand.Position = Position + 1
 WHERE EXISTS (
  SELECT *
  FROM (SELECT * FROM ChrominoInHand) AS t2
  WHERE t2.id > ChrominoInHand.id AND t2.Position=ChrominoInHand.Position and t2.GameId = ChrominoInHand.GameId and t2.PlayerId = ChrominoInHand.PlayerId)
END  


