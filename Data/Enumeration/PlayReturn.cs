namespace Data.Enumeration
{
    public enum PlayReturn
    {
        Ok = 1,
        LastChrominoIsCameleon,
        NotFree,
        NotMinTwoSameColors,
        DifferentColorsAround,
        NotPlayerTurn,
        DrawChromino,
        SkipTurn,
        GameFinish,
        ErrorGameFinish,
        CantDraw2TimesInARow,
        NoMoreChrominosInStack,
    }

    public static class PlayReturnMethods
    {
        public static bool IsError(this PlayReturn playReturn)
        {
            switch (playReturn)
            {
                case PlayReturn.LastChrominoIsCameleon:
                case PlayReturn.NotFree:
                case PlayReturn.NotMinTwoSameColors:
                case PlayReturn.DifferentColorsAround:
                case PlayReturn.NotPlayerTurn:
                case PlayReturn.ErrorGameFinish:
                case PlayReturn.CantDraw2TimesInARow:
                case PlayReturn.NoMoreChrominosInStack:
                    return true;
                default:
                    return false;
            }
        }
    }
}
