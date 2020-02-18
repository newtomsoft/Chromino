namespace Data.Enumeration
{
    public enum Orientation
    {
        Horizontal = 1, // coté gauche : 0 ; coté haut : 1-2-3 ; cotés droit : 4 ; cotés bas : 5-6-7
        Vertical, // coté gauche : 5-6-7 ; coté haut : 0 ; cotés droit : 1-2-3 ; cotés bas : 4
        HorizontalFlip, // coté gauche : 4 ; coté haut : 5-6-7; cotés droit : 0 ; cotés bas : 1-2-3
        VerticalFlip, // coté gauche : 1-2-3 ; coté haut : 4 ; cotés droit : 5-6-7 ; cotés bas : 0
    }
}
