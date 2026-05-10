using System;

namespace PickMeUp.Data
{
    [Serializable]
    public class RuntimeItem
    {
        public string itemId;
        public string instanceId; // unique per drop
        public int quantity = 1;
        public int upgradeLevel = 0;
        public string equippedHeroId = ""; // empty if not equipped

        public RuntimeItem() { }

        public RuntimeItem(string itemId, string instanceId, int quantity = 1)
        {
            this.itemId = itemId;
            this.instanceId = instanceId;
            this.quantity = quantity;
            this.upgradeLevel = 0;
            this.equippedHeroId = "";
        }

        public bool IsEquipped => !string.IsNullOrEmpty(equippedHeroId);
    }
}
