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
public class EnhancementDTO
{
    public string Name;
    public List<string> Description;
    public List<ModifierDTO> modifiers;
    public static EnhancementDTO GetEnhancementDTO(InventoryItem enchantment, ValueHelpers helpers)
    {   
        var enhancement = AssetAccess.GetAsset(enchantment.itemDefinition.appliesEnchantment);
        List<ModifierDTO> itemModifiers = new();
        foreach (var mod in enhancement.modifiersApplied)
        {
            var attributeExpanded = AssetAccess.GetAsset(mod.attribute);
            itemModifiers.Add(new ModifierDTO
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
                projEffectDefinition = helpers.GetProjDTO(attributeExpanded?.projEffectDefinition),
                customVisualsPrefab = attributeExpanded.customVisualsPrefab?.ToString(),
                applyAttributeModifier = attributeExpanded.applyAttributeModifier?.ToString(),
                explosionOnHit = attributeExpanded.explosionOnHit.ToString(),
                explosionScale = attributeExpanded.explosionScale,
                spawnOnUnitHit = helpers.GetEffectSpawnDTO(attributeExpanded?.spawnOnUnitHit),
                spawnOnEnvironmentHit = helpers.GetEffectSpawnDTO(attributeExpanded?.spawnOnEnvironmentHit),
                spawnOnStartShoot = helpers.GetEffectSpawnDTO(attributeExpanded?.spawnOnStartShoot),
                bloodDecalOnEnvironmentHit = attributeExpanded.bloodDecalOnEnvironmentHit.ToString(),
                replacesBloodType = attributeExpanded.replacesBloodType.ToString()
            });
        }

        InventoryUI enchUI = helpers.GetInventoryUI(enchantment);
        enchUI.itemDescription.Setup(enchantment);

        return new EnhancementDTO
        {
            Name = enchantment.itemDefinition.LocalizedDisplayName,
            Description = helpers.GetDescriptionText(enchUI.itemDescription),
            modifiers = itemModifiers
        };
        
    }
}

[Serializable]
public class ModifierDTO
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
    public ProjectileDTO projEffectDefinition;
    public string customVisualsPrefab = "";
    public string applyAttributeModifier = "None +0%";
    public string explosionOnHit = "None";
    public float explosionScale = 1.0f;
    public EffectSpawnDTO spawnOnUnitHit;
    public EffectSpawnDTO spawnOnEnvironmentHit;
    public EffectSpawnDTO spawnOnStartShoot;
    public string bloodDecalOnEnvironmentHit = "Normal";
    public string replacesBloodType = "Normal";
}

public class ProjectileDTO
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

public class EffectSpawnDTO
{
    public string effect;
    public float procChance;
}