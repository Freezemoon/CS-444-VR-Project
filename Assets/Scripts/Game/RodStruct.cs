using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Ici est défini toutes les cannes à pêches du jeu ainsi que leur stats.
    /// </summary>
    public class FishingRodStats
    {
        public string Name;
        public float MaxLineLength;
        public float ReelSpeed;
        public int Price;

        public FishingRodStats(string name, float maxLineLength, float reelSpeed, int price)
        {
            Name = name;
            MaxLineLength = maxLineLength;
            ReelSpeed = reelSpeed;
            Price = price;
        }
    }

    // Librairie des cannes actuellement dans le jeu
    public static class FishingRodLibrary
    {
        public static readonly FishingRodStats Basic = new("Wooden Rod", 10f, 1.0f, 0);
        public static readonly FishingRodStats Advanced = new("Quality Rod", 15f, 1.5f, 150);
        public static readonly FishingRodStats Master = new("Master Rod", 20f, 2.0f, 250);

        public static IEnumerable<FishingRodStats> all =>
            new[] { Basic, Advanced, Master };
    }
}