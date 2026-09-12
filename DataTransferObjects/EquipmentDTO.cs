using System;
using System.Collections.Generic;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.CharacterStats;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.Effects;
using PerfectRandom.Sulfur.Core.UI;
using UnityEngine;
using PerfectRandom.Sulfur.Core.UI.ItemDescription;
using PerfectRandom.Sulfur.Core.UI.Inventory;


[Serializable]
public class EquipmentDTO
{
    public string name;
    public ItemId id;
    public List<string> description;
    public string flavorText;
    public string type;
    public string slotType;
    public int priceBuy;
    public int priceSell;
    public int InventorySizeX;
    public int InventorySizeY;
    public int maxDurability;
    public List<EquipmentModifierDTO> modifiersOnEquipNew;
}

public class EquipmentModifierDTO
{
    public string attribute;
    public string modType;
    public float value;
    public static List<EquipmentModifierDTO> GetEquipmentModifierDTO(InventoryItem equipment, ValueHelpers helpers)
    {
        List<EquipmentModifierDTO> modifierList = [];

        foreach (var mod in equipment.itemDefinition.modifiersOnEquipNew)
        {
            modifierList.Add(new EquipmentModifierDTO
            {
                attribute = mod.attribute.ToString(),
                modType = helpers.FromStatModTypeToString(mod.modType),
                value = mod.value
            });
        }
        return modifierList;
    }
}