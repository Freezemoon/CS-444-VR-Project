using Game;

namespace Game
{
    /// <summary>
    /// Permet de stocker l'état du jeu
    /// </summary>
    public class GameState
    {
        public int BucketValue = 1000; // valeur des poissons pas encore cash in
        public int Money = 0; // monnaie actuelle en posession du joueur
        public FishingRodStats CurrentRod = FishingRodLibrary.Basic; // Canne actuelle du joueur

        public int BBBaitCount = 1;
        public int BGBaitCount = 1;
        public int BRBaitCount = 1;
        public int GBBaitCount = 1;
        public int GGBaitCount = 1;
        public int GRBaitCount = 1;
        public int RBBaitCount = 1;
        public int RGBaitCount = 1;
        public int RRBaitCount = 1;
        
        public int DynamiteAmount;

        public int EasyFishCought;
        public int MediumFishCought;
        public int HardFishCought;
    }
}
