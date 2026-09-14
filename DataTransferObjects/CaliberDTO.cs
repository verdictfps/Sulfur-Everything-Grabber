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
public class CaliberDTO
{
    public string name;
    public string description;
    public CaliberTypes caliberId;
    public WorldResources resourceId;
    public float baseDamage;
    public float BaseKnockback;
    public bool CanBeCaliberMmdded;
    public int numberOfProjectiles;
    public int maxAmountStored;
    public int maxAmountInBackpack;
    public int priceBuy;
    public int amountPerPickup;
    public static CaliberDTO GetCaliberDTO(CaliberType caliber, ItemDefinition caliberDef, ValueHelpers helpers)
    {   
        return new CaliberDTO
        {
            name = AssetAccess.GetAsset(caliber.usesResource).LocalizedLabel,
            description = AssetAccess.GetAsset(caliber.usesResource).LocalizedShortName,
            caliberId = caliber.id,
            resourceId = AssetAccess.GetAsset(caliber.usesResource).id,
            baseDamage = caliber.baseDamage,
            BaseKnockback = caliber.BaseKnockback,
            CanBeCaliberMmdded = caliber.CanBeCaliberModded,
            numberOfProjectiles = caliber.numberOfProjectiles,
            maxAmountStored = AssetAccess.GetAsset(caliber.usesResource).maxAmountStored,
            maxAmountInBackpack = AssetAccess.GetAsset(caliber.usesResource).maxAmountInBackpack,
            priceBuy = caliberDef.basePrice,
            amountPerPickup = caliberDef.resourceOnConsume[0].amount
        };
    }
}