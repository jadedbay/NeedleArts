using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NeedleArts;

public static class Util {
    public static Texture2D LoadTextureFromAssembly(string resourceName) {
        var asm = Assembly.GetExecutingAssembly();

        using Stream? stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null) {
            NeedleArtsPlugin.Log.LogError($"Resource not found: {resourceName}");
            return null;
        }

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        byte[] data = ms.ToArray();

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return tex;
    }

    public static tk2dSpriteAnimationClip CopyClip(tk2dSpriteAnimationClip clip, string newName) {
        return new tk2dSpriteAnimationClip {
            name = newName,
            frames = clip.frames,
            fps = clip.fps,
            loopStart = clip.loopStart,
            wrapMode = clip.wrapMode
        };
    }
    
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