using System;
using UnityEngine;

public class FactionsPanel : MonoBehaviour
{
    [SerializeField] private FactionCard brassArmyCard;
    [SerializeField] private FactionCard seraphsCard;
    [SerializeField] private FactionCard overpressureCard;

    public void Initialize(SaveData saveData, Action<Faction> onFactionCardClick)
    {
        brassArmyCard.SetLevel(saveData.brassArmySave.level);
        seraphsCard.SetLevel(saveData.seraphsSave.level);
        overpressureCard.SetLevel(saveData.overpressuSave.level);

        brassArmyCard.OnSelect += onFactionCardClick;
        seraphsCard.OnSelect += onFactionCardClick;
        overpressureCard.OnSelect += onFactionCardClick;
    }
}
