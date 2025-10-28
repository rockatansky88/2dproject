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
        if (item != null) desc += $"{item.itemName} È¹µæ! ";
        if (gold > 0) desc += $"°ñµå {gold} È¹µæ!";
        return desc;
    }
}
