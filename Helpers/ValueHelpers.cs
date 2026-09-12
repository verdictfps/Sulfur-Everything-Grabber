using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.CharacterStats;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Weapons;
using UnityEngine;
using System.Reflection;
using PerfectRandom.Sulfur.Core.UI.Inventory;
using PerfectRandom.Sulfur.Core.UI.ItemDescription;
using UnityEngine.Pool;
using System.Text.RegularExpressions;

public class ValueHelpers
{
    public Dictionary<string, float> GetCaliberRecoil(List<CaliberKickDefinition> kickPowerList)
    {
        Dictionary<string, float> recoil = [];
        foreach (var kick in kickPowerList)
        {
            recoil.Add(EnumConversion.CaliberTypeToString(kick.Caliber), kick.KickPower);
        }
        return recoil;
    }

    public float CalculatedBaseWeaponDamage(WeaponSO weaponSO, float caliberDamage)
    {
        return weaponSO.damageMultiplier * WeaponTypeDataExt.GetDamageMultiplier(weaponSO.weaponType) * caliberDamage;
    }

    public Dictionary<string, float> GetCaliberSpread(List<SpreadOverrideDefinition> spreadPerCaliber)
    {
        Dictionary<string, float> spread = [];
        foreach (var spr in spreadPerCaliber)
        {
            spread.Add(EnumConversion.CaliberTypeToString(spr.Caliber), spr.Spread);
        }
        return spread;
    }

    public List<string> IterateCompatibleAttachments(List<ItemDefinition> compatibleAttachments)
    {
        List<string> attachments = [];
        foreach (var att in compatibleAttachments)
        {
            if (attachments.Contains(att.LocalizedDisplayName))
            {
                continue;
            }
            else {
                attachments.Add(att.LocalizedDisplayName);
            }
        }
        return attachments;
    }

    public StatModifier GetRunSpeedMod(Weapon weapon)
    {
        Type targetType = weapon.GetType();
        MethodInfo methodInfo = targetType.GetMethod("GetRunSpeedModifier", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (methodInfo != null)
        {
            var speedMod = methodInfo.Invoke(weapon, null); 
            return speedMod as StatModifier;
        }
        else
        {
            Debug.LogError("Method not found!");
            return null;
        } 
    }
    public float GetAimPenalty(Weapon weapon)
    {
        Type targetType = weapon.GetType();
        MethodInfo methodInfo = targetType.GetMethod("get_aimPenalty", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (methodInfo != null)
        {
            return (float)methodInfo.Invoke(weapon, null); 
        }
        else
        {
            return 0f;
        } 
    }
    /*public List<string> GetCompatibleAttachments(Weapon weapon)
    {
        Type targetType = weapon.inventoryItem.GetType();
        MethodInfo methodInfo = targetType.GetMethod("compatibleAttachments", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (methodInfo != null)
        {
            return IterateCompatibleAttachments(methodInfo.Invoke(weapon, null) as List<ItemDefinition>); 
        }
        else
        {
            return null;
        } 
    }*/
    public List<string> GetCompatibleAttachments(Weapon weapon)
    {
        Type targetType = weapon.inventoryItem.GetType();
        FieldInfo fieldInfo = targetType.GetField("compatibleAttachments", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return IterateCompatibleAttachments((List<ItemDefinition>)fieldInfo.GetValue(weapon.inventoryItem));
        }
        else
        {
            return null;
        } 
    }
    public float GetChargeAmount(Weapon weapon)
    {
        Type targetType = weapon.GetType();
        FieldInfo fieldInfo = targetType.GetField("chargeAmount", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (float)fieldInfo.GetValue(weapon);
        }
        else
        {
            return 0f;
        } 
    }
    public MeleeCollider GetMeleeCollider(Weapon weapon)
    {
        Type targetType = weapon.GetType();
        FieldInfo fieldInfo = targetType.GetField("meleeCollider", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (MeleeCollider)fieldInfo.GetValue(weapon);
        }
        else
        {
            return null;
        } 
    }
    public float GetMeleeColliderField(MeleeCollider meleeCollider, string fieldName)
    {
        Type targetType = meleeCollider.GetType();
        FieldInfo fieldInfo = targetType.GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (float)fieldInfo.GetValue(meleeCollider);
        }
        else
        {
            return 0f;
        } 
    }
    public float GetBeamFloat(Weapon weapon, string fieldName)
    {
        Type targetType = weapon.GetType();
        FieldInfo fieldInfo = targetType.GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (float)fieldInfo.GetValue(weapon);
        }
        else
        {
            return 0f;
        } 
    }
    public int GetBeamInt(Weapon weapon, string fieldName)
    {
        Type targetType = weapon.GetType();
        FieldInfo fieldInfo = targetType.GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (int)fieldInfo.GetValue(weapon);
        }
        else
        {
            return 0;
        } 
    }
    public bool GetBeamBool(Weapon weapon, string fieldName)
    {
        Type targetType = weapon.GetType();
        FieldInfo fieldInfo = targetType.GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (bool)fieldInfo.GetValue(weapon);
        }
        else
        {
            return false;
        } 
    }
    public string FromStatModTypeToString(StatModType modtype) => modtype switch
    {
        StatModType.Flat        => "Flat",
        StatModType.PercentAdd  => "PercentAdd",
        StatModType.PercentMult => "PercentMult",
        _ => throw new ArgumentOutOfRangeException(nameof(modtype), $"Not expected direction value: {modtype}"),
    };

    public ProjectileDTO GetProjDTO(ProjectileEffectDefinition projEffectDefinition)
    {
        if (!projEffectDefinition)
        {
            return null;
        }
        return new ProjectileDTO
        {
            drawDefaultBullet = projEffectDefinition.drawDefaultBullet,
            mainColor = projEffectDefinition.mainColor.ToString(),
            coreColor = projEffectDefinition.coreColor.ToString(),
            playImpactSounds = projEffectDefinition.playImpactSounds,
            soundShotSilencedVolumeDb = projEffectDefinition.soundShotSilencedVolumeDb,
            innerBeamWidth = projEffectDefinition.innerBeamWidth,
            outerBeamWidth = projEffectDefinition.outerBeamWidth
        };
    }
    public AttachmentProjectileDTO GetAttProjDTO(ProjectileEffectDefinition projEffectDefinition)
    {
        if (!projEffectDefinition)
        {
            return null;
        }
        return new AttachmentProjectileDTO
        {
            drawDefaultBullet = projEffectDefinition.drawDefaultBullet,
            mainColor = projEffectDefinition.mainColor.ToString(),
            coreColor = projEffectDefinition.coreColor.ToString(),
            playImpactSounds = projEffectDefinition.playImpactSounds,
            soundShotSilencedVolumeDb = projEffectDefinition.soundShotSilencedVolumeDb,
            innerBeamWidth = projEffectDefinition.innerBeamWidth,
            outerBeamWidth = projEffectDefinition.outerBeamWidth
        };
    }
    
    public EffectSpawnDTO GetEffectSpawnDTO(EffectSpawnEntry effect)
    {
        if (!effect.effect)
        {
            return null;
        }
        return new EffectSpawnDTO
        {
            effect = effect?.effect?.ToString() ?? "",
            procChance = effect.procChance
        };
    }
    public AttachmentEffectSpawnDTO GetAttachmentEffectSpawnDTO(EffectSpawnEntry effect)
    {
        if (!effect.effect)
        {
            return null;
        }
        return new AttachmentEffectSpawnDTO
        {
            effect = effect?.effect?.ToString() ?? "",
            procChance = effect.procChance
        };
    }

    public InventoryUI GetInventoryUI(InventoryItem enchantment)
    {
        Type targetType = enchantment.GetType();
        FieldInfo fieldInfo = targetType.GetField("inventoryUI", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return (InventoryUI)fieldInfo.GetValue(enchantment);
        }
        else
        {
            return null;
        } 
    }
    public List<string> GetDescriptionText(ItemDescription itemDescription)
    {
        Type targetType = itemDescription.GetType();
        FieldInfo attributesField = targetType.GetField("attributesInUse", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo descriptionField = targetType.GetField("descriptionTextInUse", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo attachmentField = targetType.GetField("attachmentInUse", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo enchantmentField = targetType.GetField("enchantmentInUse", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        List<ItemDescriptionAttribute> attributesList = (List<ItemDescriptionAttribute>)attributesField.GetValue(itemDescription);
        List<ItemDescriptionText> descriptionList = (List<ItemDescriptionText>)descriptionField.GetValue(itemDescription);
        List<ItemDescriptionText> attachmentList = (List<ItemDescriptionText>)attachmentField.GetValue(itemDescription);
        List<ItemDescriptionText> enchantmentList = (List<ItemDescriptionText>)enchantmentField.GetValue(itemDescription);

        List<string> descriptionStrings = [];

        string pattern = @"(.*?)\((.*?)\/(.*?)\)(.*?)";
        
        if (attributesList != null) {
            foreach (var desc in attributesList)
            {
                bool isTrue = Regex.IsMatch(desc.ToString(), pattern);
                if (isTrue == true) continue;
                descriptionStrings.Add(desc.ToString());
            }
        }
        if (descriptionList != null) {
            foreach (var desc in descriptionList)
            {
                if (desc.ToString() == "Drag this item onto a weapon with an empty enchantment slot to enchant it.") continue;
                if (desc.ToString() == "Enchantment") continue;
                if (desc.ToString() == "Elemental enchantment") continue;
                if (desc.ToString() == "Attachment ") continue;
                descriptionStrings.Add(desc.ToString());
            }
        }
        if (enchantmentList != null) {
            foreach (var desc in enchantmentList)
            {
                descriptionStrings.Add(desc.ToString());
            }
        }
        if (attachmentList != null) {
            foreach (var desc in attachmentList)
            {
                descriptionStrings.Add(desc.ToString());
            }
        }

        ClearDescriptionText(itemDescription);

        if (descriptionStrings != null)
        {
            return descriptionStrings;
        }
        else
        {
            return null;
        } 
    }
    public void ClearDescriptionText(ItemDescription itemDescription)
    {
        Type targetType = itemDescription.GetType();
        MethodInfo methodInfo = targetType.GetMethod("ClearDescription", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (methodInfo != null)
        {
            methodInfo.Invoke(itemDescription, null); 
        }
    }
    public int GetPriceBuy(InventoryItem item)
    {
        if (!item) return 0;
        int priceBuyRaw = item.PriceBuy;
        bool endsWithNine = (priceBuyRaw % 10 == 9);
        if (endsWithNine == true)
        {
            priceBuyRaw += 1;
        }
        return priceBuyRaw;
    }
    public int GetPriceSell(InventoryItem item)
    {
        if (!item) return 0;
        int priceSellRaw = item.PriceSell;
        bool endsWithNine = (priceSellRaw % 10 == 9);
        if (endsWithNine == true)
        {
            priceSellRaw += 1;
        }
        return priceSellRaw;
    }
}