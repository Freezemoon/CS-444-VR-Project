using Game;

namespace Game
{
    /// <summary>
    /// Permet de stocker l'état du jeu
    /// </summary>
    public class GameState
    {
        public int BucketValue = 20; // valeur des poissons pas encore cash in
        public int Money = 0; // monnaie actuelle en posession du joueur
        public FishingRodStats CurrentRod = FishingRodLibrary.Basic; // Canne actuelle du joueur
        public int Component1Amount = 0;
        public int Component2Amount = 0;
        public int Component3Amount = 0;
        public int Component4Amount = 0;
        public int Component5Amount = 0;
        public int Component6Amount = 0;
        public int DynamiteAmount = 0;
    }
}
