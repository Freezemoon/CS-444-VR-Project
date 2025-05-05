using UnityEngine;

// Make sure to use the same namespace as GameManager

namespace Game
{
    public class TestButtons : MonoBehaviour
    {
        public void AddMoneyToBucket(int amount)
        {
            GameManager.instance.AddMoney(amount);
            Debug.Log($"You added {amount} to the bucket's value.");
        }

        public void SellBucket()
        {
            int value = GameManager.instance.GetBucketValue();
            GameManager.instance.AddMoney(value);
            GameManager.instance.AddToBucket(-value); // reset bucket
            Debug.Log($"Bucket sold, the player now has {GameManager.instance.GetMoney()}.");
        }
    
        public void ChangeRod()
        {
            GameManager.instance.ChangeFishingRod(FishingRodLibrary.Advanced);
            FishingRodStats rod = GameManager.instance.GetPlayerRod();
            Debug.Log($"Your rod is now {rod.Name}");
        }
    }
}