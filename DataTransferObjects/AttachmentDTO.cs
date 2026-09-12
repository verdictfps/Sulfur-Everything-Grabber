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
public class AttachmentDTO
{
    public string name;
    public ItemId id;
    public List<string> description;
    public string flavor;
    public int priceBuy;
    public int priceSell;
    public int InventorySizeX;
    public int InventorySizeY;
    public List<AttachmentModifierDTO> modifiers;
    public static AttachmentDTO GetAttachmentDTO(InventoryItem attachment, ValueHelpers helpers)
    {   
        List<ItemModifierContainer> enhancement = attachment.itemDefinition.modifiersOnAttachToItem;
        List<AttachmentModifierDTO> itemModifiers = new();
        foreach (var mod in enhancement)
        {
            var attributeExpanded = AssetAccess.GetAsset(mod.attribute);
            itemModifiers.Add(new AttachmentModifierDTO
            { 
                modifierName = mod.attribute.ToString(),
                statModType = helpers.FromStatModTypeToString(mod.modType),
                value = mod.value,
                id = attributeExpanded.id,
                label = attributeExpanded.label,
                itemDescriptionName = attributeExpanded.itemDescriptionName,
                showInItemDescription = attributeExpanded.showInItemDescription,
                unitMeasure = attributeExpanded.unitMeasure,
                simplifiedModAmount = attributeExpanded.simplifiedModAmount,
                simplifiedIncreaseString = attributeExpanded.simplifiedIncreaseString,
                simplifiedDecreaseString = attributeExpanded.simplifiedDecreaseString,
                isBooleanAttribute = attributeExpanded.isBooleanAttribute,
                isPercentageAttribute = attributeExpanded.isPercentageAttribute,
                showPercentageAsFactor = attributeExpanded.showPercentageAsFactor,
                overrideUnitName = attributeExpanded.overrideUnitName,
                projEffectDefinition = helpers.GetAttProjDTO(attributeExpanded?.projEffectDefinition),
                customVisualsPrefab = attributeExpanded.customVisualsPrefab?.ToString(),
                applyAttributeModifier = attributeExpanded.applyAttributeModifier?.ToString(),
                explosionOnHit = attributeExpanded.explosionOnHit.ToString(),
                explosionScale = attributeExpanded.explosionScale,
                spawnOnUnitHit = helpers.GetAttachmentEffectSpawnDTO(attributeExpanded?.spawnOnUnitHit),
                spawnOnEnvironmentHit = helpers.GetAttachmentEffectSpawnDTO(attributeExpanded?.spawnOnEnvironmentHit),
                spawnOnStartShoot = helpers.GetAttachmentEffectSpawnDTO(attributeExpanded?.spawnOnStartShoot),
                bloodDecalOnEnvironmentHit = attributeExpanded.bloodDecalOnEnvironmentHit.ToString(),
                replacesBloodType = attributeExpanded.replacesBloodType.ToString()
            });
        }

        InventoryUI enchUI = helpers.GetInventoryUI(attachment);
        enchUI.itemDescription.Setup(attachment);

        return new AttachmentDTO
        {
            name = attachment.itemDefinition.LocalizedDisplayName,
            id = attachment.itemDefinition.id,
            description = helpers.GetDescriptionText(enchUI.itemDescription),
            flavor = attachment.itemDefinition.LocalizedFlavor,
            priceBuy = attachment.PriceBuy,
            priceSell = attachment.PriceSell,
            InventorySizeX = attachment.InventorySize.x,
            InventorySizeY = attachment.InventorySize.y,
            modifiers = itemModifiers,
        };
        
    }
}

[Serializable]
public class AttachmentModifierDTO
{
    public string modifierName;
    public string statModType;
    public float value;
    public ItemAttributes id;
    public string label = "";
    public string itemDescriptionName = "";
    public bool showInItemDescription;
    public string unitMeasure = "";
    public bool simplifiedModAmount;
    public string simplifiedIncreaseString = "";
    public string simplifiedDecreaseString = "";
    public bool isBooleanAttribute;
    public bool isPercentageAttribute;
    public bool showPercentageAsFactor;
    public string overrideUnitName = "";
    public AttachmentProjectileDTO projEffectDefinition;
    public string customVisualsPrefab = "";
    public string applyAttributeModifier = "None +0%";
    public string explosionOnHit = "None";
    public float explosionScale = 1.0f;
    public AttachmentEffectSpawnDTO spawnOnUnitHit;
    public AttachmentEffectSpawnDTO spawnOnEnvironmentHit;
    public AttachmentEffectSpawnDTO spawnOnStartShoot;
    public string bloodDecalOnEnvironmentHit = "Normal";
    public string replacesBloodType = "Normal";
}

public class AttachmentProjectileDTO
{
    public ProjectileEffect id;
    public bool drawDefaultBullet;
    public string mainColor;
    public string coreColor;
    public bool playImpactSounds = true;
    public float soundShotSilencedVolumeDb;
    public float innerBeamWidth = 0.5f;
    public float outerBeamWidth = 1f;
}

public class AttachmentEffectSpawnDTO
{
    public string effect;
    public float procChance;
}