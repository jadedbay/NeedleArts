using System.Collections.Generic;

namespace NeedleArts.Utils;

public static class ToolItemManagerUtil {
    public static void AutoEquip(string crestName, ToolItem tool)
    {
        if (tool == null)
        {
            return;
        }
        string currentCrestID = crestName;
        ToolCrest crestByName = ToolItemManager.GetCrestByName(currentCrestID);
        List<ToolItem> equippedToolsForCrest = ToolItemManager.GetEquippedToolsForCrest(currentCrestID);
        List<string> list = new List<string>(crestByName.Slots.Length);
        for (int i = 0; i < crestByName.Slots.Length; i++)
        {
            ToolItem toolItem = ((equippedToolsForCrest != null && i < equippedToolsForCrest.Count) ? equippedToolsForCrest[i] : null);
            list.Add((toolItem != null) ? toolItem.name : string.Empty);
        }
        int num;
        if (tool.Type == ToolItemType.Skill)
        {
            num = -1;
            for (int j = 0; j < crestByName.Slots.Length; j++)
            {
                ToolCrest.SlotInfo slotInfo = crestByName.Slots[j];
                if (slotInfo.Type == ToolItemType.Skill && slotInfo.AttackBinding == AttackToolBinding.Neutral)
                {
                    num = j;
                    break;
                }
            }
        }
        else
        {
            int num2 = -1;
            int num3 = -1;
            for (int k = 0; k < crestByName.Slots.Length; k++)
            {
                if (crestByName.Slots[k].Type == tool.Type)
                {
                    num2 = k;
                    if (string.IsNullOrEmpty(list[k]))
                    {
                        num3 = k;
                    }
                }
            }
            num = ((num3 >= 0) ? num3 : num2);
        }
        if (num >= 0)
        {
            list[num] = tool.name;
        }
        ToolItemManager.UnlockedTool = tool;
        InventoryPaneList.SetNextOpen("Tools");
        ToolItemManager.SetEquippedTools(currentCrestID, list);
        ToolItemManager.SendEquippedChangedEvent();
    }

}