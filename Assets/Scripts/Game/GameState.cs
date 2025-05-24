using Game;

namespace Game
{
    /// <summary>
    /// Permet de stocker l'état du jeu
    /// </summary>
    public class GameState
    {
        public int BucketValue = 0; // valeur des poissons pas encore cash in
        public int Money = 0; // monnaie actuelle en posession du joueur
        public FishingRodStats CurrentRod = FishingRodLibrary.Basic; // Canne actuelle du joueur

        public int EquippedBaitStrength;
        public int EquippedBaitDurability;

        public int BBBaitCount;
        public int BGBaitCount;
        public int BRBaitCount;
        public int GBBaitCount;
        public int GGBaitCount;
        public int GRBaitCount;
        public int RBBaitCount;
        public int RGBaitCount;
        public int RRBaitCount;
        
        public int DynamiteAmount;

        public int EasyFishCought;
        public int MediumFishCought;
        public int HardFishCought;
    }
}
