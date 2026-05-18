using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance { get; private set; }

    [SerializeField] private long startingCopper = 100;
    private long totalCopper;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        totalCopper = startingCopper;
    }

    public int Copper  => (int)(totalCopper % 100);
    public int Silver  => (int)((totalCopper / 100) % 100);
    public int Gold    => (int)((totalCopper / 10000) % 100);
    public int Platinum => (int)(totalCopper / 1000000);

    public long TotalCopper => totalCopper;

    public string FormatCurrency()
    {
        if (Platinum > 0) return $"{Platinum}Platinum {Gold}Gold {Silver}Sliver {Copper}Copper";
        if (Gold > 0)     return $"{Gold}Gold {Silver}Silver {Copper}Copper";
        if (Silver > 0)   return $"{Silver}Silver {Copper}Copper";
        return $"{Copper}Copper";
    }

    public bool CanAfford(long copperAmount) => totalCopper >= copperAmount;

    public bool Spend(long copperAmount)
    {
        if (!CanAfford(copperAmount)) return false;
        totalCopper -= copperAmount;
        return true;
    }

    public void Earn(long copperAmount) => totalCopper += copperAmount;

    public void EarnFromEnemy(int monsterLevel) => Earn(Random.Range(5, 15) * (monsterLevel + 1));

    public void SetTotalCopper(long amount) => totalCopper = amount;
}
