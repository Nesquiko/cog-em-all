using System;
using UnityEngine;

public class TowerSellManager : MonoBehaviour
{
    public event Action<TowerTypes> OnSellTower;

    public void RequestSell(ITowerSellable tower)
    {
        if (tower == null) return;

        OnSellTower.Invoke(tower.TowerType());
    }
}
