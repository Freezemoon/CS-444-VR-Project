using UnityEngine;

public class BaitStat : MonoBehaviour
{
    public int level;
    private int durability = 1;
    private int strength = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (durability == 0)
        {
            //Destroy(self);
        }

    }

    void Awake()
    {
        durability = level % 10 * 3;
        strength = level / 10;
    }
}
