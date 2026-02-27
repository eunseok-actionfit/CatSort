using UnityEngine;


[CreateAssetMenu(menuName="Shop/IconTable")]
public sealed class ShopIconTable : ScriptableObject
{
    [Header("Coin (by amount tier)")]
    public Sprite coin_1000;
    public Sprite coin_5000;
    public Sprite coin_10000;
    public Sprite coin_25000;
    public Sprite coin_50000;
    public Sprite coin_100000;

    [Header("Lives")]
    public Sprite heart;

    [Header("Boosters")]
    public Sprite boosterUndo;
    public Sprite boosterShuffle;
    public Sprite boosterAddColumn;

    [Header("Remove Ads")]
    public Sprite removeAds;

    public Sprite GetCoinSprite(int amount)
    {
        if (amount >= 100000) return coin_100000;
        if (amount >= 50000)  return coin_50000;
        if (amount >= 25000)  return coin_25000;
        if (amount >= 10000)  return coin_10000;
        if (amount >= 5000)   return coin_5000;
        return coin_1000;
    }
}