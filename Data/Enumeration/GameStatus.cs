namespace Data.Enumeration
{
    public enum GameStatus
    {
        InProgress = 1,
        SingleFinished,
        Finished,
        SingleInProgress,
    }

    public static class GameStatusInfo
    {
        public static bool IsFinish(this GameStatus gameStatus)
        {
            switch (gameStatus)
            {
                case GameStatus.Finished:
                case GameStatus.SingleFinished:
                    return true;
                default:
                    return false;
            }
        }
    }
}
