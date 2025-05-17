using Game;

namespace Game
{
    /// <summary>
    /// Permet de stoquer l'état du jeu
    /// </summary>
    public class GameState
    {
        public int BucketValue = 20; // valeur des poissons pas encore cash in
        public int Money = 0; // monnaie actuelle en posession du joueur
        public FishingRodStats CurrentRod = FishingRodLibrary.Basic; // Canne actuelle du joueur
    }
}
