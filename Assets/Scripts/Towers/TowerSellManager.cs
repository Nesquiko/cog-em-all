using System;
using UnityEngine;

public class TowerSellManager : MonoBehaviour
{
    public event Action<ITower> OnSellTower;

    public void RequestSell(ITowerSellable tower)
    {
        tower.SellAndDestroy();
        OnSellTower.Invoke(tower);
    }
}
