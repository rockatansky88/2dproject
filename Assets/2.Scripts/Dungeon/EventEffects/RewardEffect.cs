public class RewardEffect : IEventEffect
{
    private ItemDataSO item;
    private int gold;

    public void Apply(Party party)
    {
        //if (item != null)
        //{
        //    InventoryManager.Instance.AddItem(item, 1);
        //}

        //if (gold > 0)
        //{
        //    GameManager.Instance.AddGold(gold);
        //}
    }

    public string GetDescription()
    {
        string desc = "";
        if (item != null) desc += $"{item.itemName} ȹ��! ";
        if (gold > 0) desc += $"��� {gold} ȹ��!";
        return desc;
    }
}
