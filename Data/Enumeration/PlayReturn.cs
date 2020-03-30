namespace Data.Enumeration
{
    public enum PlayReturn
    {
        Ok = 1,
        LastChrominoIsCameleon,
        NotFree,
        NotTwoOrMoreSameColors,
        DifferentColorsAround,
        NotPlayerTurn,
        DrawChromino,
        SkipTurn,
    }

    public static class PlayReturnMethods
    {
        public static bool IsError(this PlayReturn playReturn)
        {
            switch (playReturn)
            {
                case PlayReturn.LastChrominoIsCameleon:
                case PlayReturn.NotFree:
                case PlayReturn.NotTwoOrMoreSameColors:
                case PlayReturn.DifferentColorsAround:
                case PlayReturn.NotPlayerTurn:
                    return true;
                default:
                    return false;
            }
        }
    }
}
