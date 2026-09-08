using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.CharacterStats;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Weapons;
using UnityEngine;
using PerfectRandom.Sulfur.Core.Units;
// using I2.Loc;
using HarmonyLib;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.UI;
using PerfectRandom.Sulfur.Core.UI.Inventory;
using PerfectRandom.Sulfur.Core.DevTools;
using PerfectRandom.Sulfur.Core.UI.ItemDescription;

namespace EverythingDataGrabber;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    CaliberType[] Caliberdatabase;
    List<ItemDefinition> itemDatabase;
    DatabaseGrabber grabber = new();
    private static List<ItemDefinition> weaponList = [];
    private static List<BaseDTO> weaponPropertyList = [];
    private static List<ItemDefinition> enchantmentList = [];
    private static List<EnhancementDTO> oilList = [];
    private static List<EnhancementDTO> scrollList = [];
    private static bool itHasBegun = false;
    private ConfigEntry<KeyboardShortcut> GrabWeapons { get; set; }
    private ConfigEntry<KeyboardShortcut> GrabEnchantments { get; set; }
    private ConfigEntry<KeyboardShortcut> GrabEquipment { get; set; }
    public static InventoryUI inventoryUI { get; set; }

    private void Awake()
    {
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(); 
        GrabWeapons = Config.Bind("Hotkeys", "Start Weapon Grabbing", new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift));
        GrabEnchantments = Config.Bind("Hotkeys", "Start Enchantment Grabbing", new KeyboardShortcut(KeyCode.I, KeyCode.LeftShift));
        GrabEquipment = Config.Bind("Hotkeys", "Start Equipment Grabbing", new KeyboardShortcut(KeyCode.O, KeyCode.LeftShift));
        Debug.Log("[ Mod: EverythingGrabber ] Plugin loaded successfully");
    }

    private void Update()
    {
        if (GrabWeapons.Value.IsDown())
        {
            StartCoroutine(SpawnWeapons());
        }
        if (GrabEnchantments.Value.IsDown())
        {
            StartCoroutine(SpawnEnchantments());
        }
    }

    [HarmonyPatch(typeof(Weapon), "Initialize")]
    public class WeaponStatsInterceptor
    {
        static void Postfix(object __instance)
        {
            Debug.Log("[ Mod: EverythingGrabber ] Postfix found a weapon");
            if (__instance == null) return;
            Weapon weapon = __instance as Weapon;
            if (weapon == null)
            {
                return;
            }

            weapon.inventoryItem.ModifyDurability(10000000f);

            BaseDTO returnDTO = GetRelevantDTO(weapon);

            if (returnDTO == null) return;
            if (itHasBegun == false) return;

            bool exists = weaponPropertyList.Any(item => item.Name == returnDTO.Name);

            if (exists == true)
            {
                return;
            }
            
            ImageHelpers.SaveBaseImageWeapon(weapon, returnDTO);

            weaponPropertyList.Add(returnDTO);
        }
    }

    [HarmonyPatch(typeof(InventoryItem), "Start")]
    public class EnchantmentStatsInterceptor
    {
        static void Postfix(object __instance)
        {
            if (__instance == null) return;
            InventoryItem enchantment = __instance as InventoryItem;
            
            if (enchantment?.itemDefinition?.ItemType != ItemType.Enchantment) return;
            if (!enchantment.itemDefinition.includedInEarlyAccess) return;
            if (enchantment.itemDefinition.LocalizedDisplayName.StartsWith("Test")) return;

            Debug.Log("[ Mod: EverythingGrabber ] Postfix found an enchantment");

            if (itHasBegun == false)
            {
                Debug.Log("[ Mod: EverythingGrabber ] idk why but ithasbegun is false");
                return;
            }
            if (enchantment == null)
            {
                Debug.Log("[ Mod: EverythingGrabber ] For some fuckiing reason enchantment is null");
                return;
            }

            var helper = new ValueHelpers();
            EnhancementDTO returnDTO = EnhancementDTO.GetEnhancementDTO(enchantment, helper);

            if (returnDTO == null) 
            { 
                Debug.Log("[ Mod: EverythingGrabber ] ReturnDTO failed");
                return;
            }
            if (returnDTO.modifiers.Count == 0) 
            {
                Debug.Log("[ Mod: EverythingGrabber ] WHY WOULD MODIFIERS BE ZERO");
                return;
            }
            if (returnDTO.Name.Contains("Oil"))
            {
                oilList.Add(returnDTO);
                ImageHelpers.SaveBaseImageEnch(enchantment.itemDefinition, "Oils");
            }
            else
            {
                scrollList.Add(returnDTO);
                ImageHelpers.SaveBaseImageEnch(enchantment.itemDefinition, "Scrolls");
            }
            ClearInventory();
        }
    }

    [HarmonyPatch(typeof(InventoryUI), "Start")]
    public class InventoryInterceptor
    {
        public static void Postfix(object __instance)
        {
            Debug.Log("[ Mod: EverythingGrabber ] Postfix found player inventory");
            
            if (__instance == null) return;
            Plugin.inventoryUI = __instance as InventoryUI;
            if (Plugin.inventoryUI == null)
            {
                return;
            }
        }
    }

    private IEnumerator Start()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        while (!StaticInstance<AsyncAssetLoading>.Instance.loadingDone) yield return new WaitForEndOfFrame();

        while (GameManager.Instance == null) yield return new WaitForEndOfFrame();
        while (GameManager.Instance.awaitingStartLevel) yield return new WaitForEndOfFrame();

        itemDatabase = grabber.GetListOfItemDefinitions();
        Caliberdatabase = DatabaseGrabber.GetCaliberDatabase();

        foreach (var itemDef in itemDatabase)
        {
            if (itemDef?.slotType != SlotType.Weapon & itemDef?.slotType != SlotType.BasicMelee & itemDef?.slotType != SlotType.Gadget) continue;
            if (itemDef is not WeaponSO) continue;
            if (itemDef?.prefab == null) continue;
            if (itemDef?.showcasePrefab == null) continue;

            var weaponSO = itemDef as WeaponSO;
            weaponSO?.alwaysSpawnWithFullDurability = true;
            weaponList.Add(weaponSO);
        }
        foreach (var itemDef in itemDatabase)
        {
            if (itemDef?.ItemType != ItemType.Enchantment) continue;
            if (!itemDef.includedInEarlyAccess) continue;
            if (itemDef.LocalizedDisplayName.StartsWith("Test")) continue;

            enchantmentList.Add(itemDef);
        }
    }

    private IEnumerator SpawnWeapons()
    {
        if (weaponList.Count == 0) yield break;

        ClearSlots();
        ClearInventory();

        itHasBegun = true;

        SpawnHelper.SetupWeaponSpawning();

        foreach (var weapon in weaponList)
        {
            InventorySlot slot = SpawnHelper.ToInventorySlot(weapon.slotType);
            if (!SpawnHelper.IsWeaponSlotEmpty(slot) & SpawnHelper.GetItemInSlot(slot)?.SlotType != SlotType.BasicMelee)
            {
                SpawnHelper.GetItemInSlot(slot)?.DropFromPlayer();
            }
            else if (!SpawnHelper.IsWeaponSlotEmpty(slot) & SpawnHelper.GetItemInSlot(slot)?.SlotType == SlotType.BasicMelee)
            {
                SpawnHelper.RemoveGeneratedWeaponSafely(SpawnHelper.GetItemInSlot(slot), "Melee weapons don't drop safely");
            }

            StaticInstance<UIManager>.Instance.InventoryUI.SpawnItemInSlot(weapon, slot, null);

            yield return null;
        }

        SaveWeapons(weaponPropertyList);

        itHasBegun = false;
    }

    private IEnumerator SpawnEnchantments()
    {
        if (enchantmentList.Count == 0) yield break;
        
        ClearSlots();
        ClearInventory();

        itHasBegun = true;

        foreach (var enchantment in enchantmentList)
        {
            InventorySlot slot = SpawnHelper.ToInventorySlot(enchantment.slotType);
            StaticInstance<DevToolsManager>.Instance.SpawnToInventory(enchantment);

            yield return null;
        }

        SaveEnchantments();
        itHasBegun = false;
    }

    private static void ClearSlots()
    {
        SpawnHelper.GetItemInSlot(InventorySlot.Weapon0)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.BasicMelee)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.Gadget0)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.Gadget1)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.Gadget2)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.Head)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.Torso)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.LeftFoot)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.RightFoot)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.PassiveEnhancement0)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.PassiveEnhancement1)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.PassiveEnhancement2)?.DropFromPlayer();
        SpawnHelper.GetItemInSlot(InventorySlot.PassiveEnhancement3)?.DropFromPlayer();
    }
    private static void ClearInventory()
    {
        if (Plugin.inventoryUI != null) {
            Plugin.inventoryUI.bagSpaceItemGrid.RemoveAllItems();
        }
    }

    private static void SaveWeapons(List<BaseDTO> weaponPropertyList)
    {
        Debug.Log(weaponPropertyList);
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CustomContractResolver(),
            Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(weaponPropertyList, settings);
        string rootDir = Paths.GameRootPath;
        string folderPath = Path.Combine(rootDir, "Extracted Data\\Weapons\\");
        Directory.CreateDirectory(folderPath);
        string path = Path.Combine(folderPath, "weaponPropertyList.json");
        File.WriteAllText(path, json);
    }

    private static void SaveEnchantments()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreUnchangedDefaultsResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        Logger.LogMessage($"Number of Oils: {oilList.Count}");
        Logger.LogMessage($"Number of Scrolls: {scrollList.Count}");

        string json = JsonConvert.SerializeObject(oilList, settings);
        string rootDir = Paths.GameRootPath;
        string folderPath = Path.Combine(rootDir, "Extracted Data\\Oils\\");
        Directory.CreateDirectory(folderPath);
        string path = Path.Combine(folderPath, "oils.json");
        File.WriteAllText(path, json);

        string json2 = JsonConvert.SerializeObject(scrollList, settings);
        string rootDir2 = Paths.GameRootPath;
        string folderPath2 = Path.Combine(rootDir2, "Extracted Data\\Scrolls\\");
        Directory.CreateDirectory(folderPath2);
        string path2 = Path.Combine(folderPath2, "scrolls.json");
        File.WriteAllText(path2, json2);
    }

    private static BaseDTO GetRelevantDTO(Weapon weapon)
    {
        var helper = new ValueHelpers();
        switch (weapon?.weaponDefinition.weaponType)
        {
            case WeaponTypes.Throwable:
                return ThrowableDTO.CreateThrowableDTO(weapon, helper);
            case WeaponTypes.Melee:
                return MeleeDTO.CreateMeleeDTO(weapon, helper);
            case null:
                return null;
            case WeaponTypes.End:
                return null;
            default: // This covers all guns.
                return WeaponDTO.CreateWeaponDTO(weapon, helper);
        }
    }
}