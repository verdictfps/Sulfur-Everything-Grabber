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
using UnityEngine.UIElements;


[Serializable]
public class ChamberDTO
{
    public string name;
    public ItemId id;
    public List<string> description;
    public string flavor;
    public int priceBuy;
    public int priceSell;
    public int InventorySizeX;
    public int InventorySizeY;
    public string modifiesCaliber;
    public static ChamberDTO GetChamberDTO(InventoryItem chamber, ValueHelpers helpers)
    {   
        InventoryUI enchUI = helpers.GetInventoryUI(chamber);
        enchUI.itemDescription.Setup(chamber);

        return new ChamberDTO
        {
            name = chamber.itemDefinition.LocalizedDisplayName,
            id = chamber.itemDefinition.id,
            description = helpers.GetDescriptionText(enchUI.itemDescription),
            flavor = chamber.itemDefinition.LocalizedFlavor,
            priceBuy = chamber.PriceBuy,
            priceSell = chamber.PriceSell,
            InventorySizeX = chamber.InventorySize.x,
            InventorySizeY = chamber.InventorySize.y,
            modifiesCaliber = EnumConversion.CaliberTypeToString(chamber.itemDefinition.modifiesCaliber)
        };
        
    }
}