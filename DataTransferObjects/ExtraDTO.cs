using System;
using System.Collections.Generic;
using System.Diagnostics;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.CharacterStats;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.Weapons;

[Serializable]
public class ExtraDTO : BaseDTO
{
    public int shotsToReachFullSpread;
    public float timeToCooldownSpread;
    public float DurabilityLossMultiplier;
    public int InventorySizeX;
    public int InventorySizeY;
    public float cooldown;
    public float cooldownBeforeReload;
    public float SpreadStrength;
    public string ProjectileType;
    public string DamageType;

    public bool isBeamgun;
    public float beamBurstTimer;
    public float beamBurstAmmoRate;
    public float beamBurstAmmoRemaining;
    public bool beamBurstReclaims;
    public float beamBurstMinDuration = 0.15f;
    public float beamBurstMaxDuration = 1.5f;
    public int maxBeamShotsPerStep = 8;
    public float beamMaxTicksPerSecond = 15f;
    public float minBeamShotInterval = 0.001f;
    public float beamAnglePerSlot = 137.5f;
    public float beamWanderDetailRate = 3f;
    public float beamWanderNormalize = 1.2f;
    public float beamSpreadSmoothRate = 6f;
    public float beamScreenShakeScale = 0.15f;
    public bool beamVisualShot;
    public bool beamCostShot;
    public int beamShotsPerCost = 1;
    public float beamAmmoPerTick = 1f;
    public float beamAmmoAccumulator;
    public float beamDamageMultiplier = 1f;
    public int beamProjectileIndex;
    public float beamSpreadSmoothed;
    public float beamShotCooldown;

    
    public static ExtraDTO SetExtraWeaponStats(Weapon weapon, ValueHelpers helpers)
    {
        return new ExtraDTO
        {
            shotsToReachFullSpread = weapon.weaponDefinition.shotsToReachFullSpread,
            timeToCooldownSpread = weapon.weaponDefinition.timeToCooldownSpread,
            DurabilityLossMultiplier = weapon.inventoryItem.DurabilityLossMultiplier,
            InventorySizeX = weapon.inventoryItem.InventorySize.x,
            InventorySizeY = weapon.inventoryItem.InventorySize.y,
            cooldown = weapon.cooldown,
            cooldownBeforeReload = weapon.cooldownBeforeReload,
            SpreadStrength = weapon.SpreadStrength,
            ProjectileType = EnumConversion.ProjectileTypeToString(weapon.ProjectileType),
            DamageType = weapon.GetDamageType().ToString(),
            isBeamgun = helpers.GetBeamBool(weapon, "isBeamgun"),
            beamBurstTimer = helpers.GetBeamFloat(weapon, "beamBurstTimer"),
            beamBurstAmmoRate = helpers.GetBeamFloat(weapon, "beamBurstAmmoRate"),
            beamBurstAmmoRemaining = helpers.GetBeamFloat(weapon, "beamBurstAmmoRemaining"),
            beamBurstReclaims = helpers.GetBeamBool(weapon, "beamBurstReclaims"),
            beamBurstMinDuration = helpers.GetBeamFloat(weapon, "beamBurstMinDuration"),
            beamBurstMaxDuration = helpers.GetBeamFloat(weapon, "beamBurstMaxDuration"),
            maxBeamShotsPerStep = helpers.GetBeamInt(weapon, "maxBeamShotsPerStep"),
            beamMaxTicksPerSecond = helpers.GetBeamFloat(weapon, "beamMaxTicksPerSecond"),
            minBeamShotInterval = helpers.GetBeamFloat(weapon, "minBeamShotInterval"),
            beamAnglePerSlot = helpers.GetBeamFloat(weapon, "beamAnglePerSlot"),
            beamWanderDetailRate = helpers.GetBeamFloat(weapon, "beamWanderDetailRate"),
            beamWanderNormalize = helpers.GetBeamFloat(weapon, "beamWanderNormalize"),
            beamSpreadSmoothRate = helpers.GetBeamFloat(weapon, "beamSpreadSmoothRate"),
            beamScreenShakeScale = helpers.GetBeamFloat(weapon, "beamScreenShakeScale"),
            beamVisualShot = helpers.GetBeamBool(weapon, "beamVisualShot"),
            beamCostShot = helpers.GetBeamBool(weapon, "beamCostShot"),
            beamShotsPerCost = helpers.GetBeamInt(weapon, "beamShotsPerCost"),
            beamAmmoPerTick = helpers.GetBeamFloat(weapon, "beamAmmoPerTick"),
            beamAmmoAccumulator = helpers.GetBeamFloat(weapon, "beamAmmoAccumulator"),
            beamDamageMultiplier = helpers.GetBeamFloat(weapon, "beamDamageMultiplier"),
            beamProjectileIndex = helpers.GetBeamInt(weapon, "beamProjectileIndex"),
            beamSpreadSmoothed = helpers.GetBeamFloat(weapon, "beamSpreadSmoothed"),
            beamShotCooldown = helpers.GetBeamFloat(weapon, "beamShotCooldown")
        };
    }
    public static ExtraDTO SetExtraMeleeStats(Weapon weapon, ValueHelpers helpers)
    {
        return new ExtraDTO
        {
            InventorySizeX = weapon.inventoryItem.InventorySize.x,
            InventorySizeY = weapon.inventoryItem.InventorySize.y,
            DamageType = weapon.GetDamageType().ToString()
        };
    }
    public static ExtraDTO SetExtraThrowableStats(Weapon weapon, ValueHelpers helpers)
    {
        return new ExtraDTO
        {
            InventorySizeX = weapon.inventoryItem.InventorySize.x,
            InventorySizeY = weapon.inventoryItem.InventorySize.y,
            ProjectileType = EnumConversion.ProjectileTypeToString(weapon.ProjectileType),
            DamageType = weapon.GetDamageType().ToString()
        };
    }
}