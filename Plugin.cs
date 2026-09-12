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
    // Logger(s)
    internal static new ManualLogSource Logger;

    // Databases & their helpers
    CaliberType[] Caliberdatabase;
    List<ItemDefinition> itemDatabase;
    DatabaseGrabber grabber = new();

    // Other stuff
    public static InventoryUI inventoryUI { get; set; }
    private static bool itHasBegun = false;

    // Weapon fields
    private static List<ItemDefinition> weaponList = [];
    private static List<BaseDTO> weaponPropertyList = [];

    // Enchantment fields
    private static List<ItemDefinition> enchantmentList = [];
    private static List<EnhancementDTO> oilList = [];
    private static List<EnhancementDTO> scrollList = [];

    // Equipment fields
    private static List<ItemDefinition> equipmentList = [];
    private static List<EquipmentDTO> armorList = [];
    private static List<EquipmentDTO> trinketList = [];

    // Attachment fields
    private static List<ItemDefinition> attachmentList = [];
    private static List<AttachmentDTO> attachmentPropertyList = [];

    // Chamber fields
    private static List<ItemDefinition> chamberList = [];
    private static List<AttachmentDTO> chamberPropertyList = [];

    // Shortcut fields
    private ConfigEntry<KeyboardShortcut> GrabWeapons { get; set; }
    private ConfigEntry<KeyboardShortcut> GrabEnchantments { get; set; }
    private ConfigEntry<KeyboardShortcut> GrabEquipment { get; set; }
    private ConfigEntry<KeyboardShortcut> GrabAttachments { get; set; }
    private ConfigEntry<KeyboardShortcut> GrabChambers { get; set; }

    private void Awake()
    {
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(); 
        GrabWeapons = Config.Bind("Hotkeys", "Start Weapon Grabbing", new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift));
        GrabEnchantments = Config.Bind("Hotkeys", "Start Enchantment Grabbing", new KeyboardShortcut(KeyCode.I, KeyCode.LeftShift));
        GrabEquipment = Config.Bind("Hotkeys", "Start Equipment Grabbing", new KeyboardShortcut(KeyCode.O, KeyCode.LeftShift));
        GrabAttachments = Config.Bind("Hotkeys", "Start Attachment Grabbing", new KeyboardShortcut(KeyCode.Y, KeyCode.LeftShift));
        GrabChambers = Config.Bind("Hotkeys", "Start Chamber Grabbing", new KeyboardShortcut(KeyCode.P, KeyCode.LeftShift));
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
        if (GrabEquipment.Value.IsDown())
        {
            StartCoroutine(SpawnEquipment());
        }
        if (GrabAttachments.Value.IsDown())
        {
            StartCoroutine(SpawnAttachments());
        }
        if (GrabChambers.Value.IsDown())
        {
            StartCoroutine(SpawnChambers());
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
                Debug.Log("[ Mod: EverythingGrabber ] Not ready for capture. Skipping...");
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
            if (returnDTO.name.Contains("Oil"))
            {
                oilList.Add(returnDTO);
                ImageHelpers.SaveBaseImage(enchantment.itemDefinition, "Oils");
            }
            else
            {
                scrollList.Add(returnDTO);
                ImageHelpers.SaveBaseImage(enchantment.itemDefinition, "Scrolls");
            }
            ClearInventory();
        }
    }
    [HarmonyPatch(typeof(InventoryItem), "Start")]
    public class EquipmentStatsInterceptor
    {
        static void Postfix(object __instance)
        {
            if (__instance == null) return;
            InventoryItem equipment = __instance as InventoryItem;

            if (!equipment) return;
            if (equipment.itemDefinition.ItemType != ItemType.Armor && equipment.itemDefinition.ItemType != ItemType.Misc) return;
            if (!equipment.itemDefinition.includedInEarlyAccess) return;
            if (equipment.itemDefinition.modifiersOnEquipNew.Count == 0) return;

            equipment.ModifyDurability(100);

            Debug.Log("[ Mod: EverythingGrabber ] Postfix found an equipment item");

            if (itHasBegun == false)
            {
                Debug.Log("[ Mod: EverythingGrabber ] Not ready for capture. Skipping...");
                return;
            }
            if (equipment == null)
            {
                Debug.Log("[ Mod: EverythingGrabber ] For some fucking reason equipment is null");
                return;
            }

            var helper = new ValueHelpers();
            List<EquipmentModifierDTO> returnDTO = EquipmentModifierDTO.GetEquipmentModifierDTO(equipment, helper);

            if (returnDTO == null) 
            { 
                Debug.Log("[ Mod: EverythingGrabber ] ReturnDTO failed");
                return;
            }

            InventoryUI enchUI = helper.GetInventoryUI(equipment);
            enchUI.itemDescription.Setup(equipment);
            
            if (equipment.itemDefinition.ItemType == ItemType.Armor)
            {
                armorList.Add(new EquipmentDTO
                {
                    name = equipment.itemDefinition.LocalizedDisplayName,
                    id = equipment.itemDefinition.id,
                    description = helper.GetDescriptionText(enchUI.itemDescription),
                    flavorText = equipment.itemDefinition.LocalizedFlavor,
                    type = "Armor",
                    slotType = equipment.itemDefinition.slotType.ToString(),
                    priceBuy = helper.GetPriceBuy(equipment),
                    priceSell = helper.GetPriceSell(equipment),
                    InventorySizeX = equipment.InventorySize.x,
                    InventorySizeY = equipment.InventorySize.y,
                    maxDurability = equipment.DurabilityMax,
                    modifiersOnEquipNew = returnDTO
                });
                ImageHelpers.SaveBaseImage(equipment.itemDefinition, "Armor");
            }
            else
            {
                trinketList.Add(new EquipmentDTO
                {
                    name = equipment.itemDefinition.LocalizedDisplayName,
                    id = equipment.itemDefinition.id,
                    description = helper.GetDescriptionText(enchUI.itemDescription),
                    flavorText = equipment.itemDefinition.LocalizedFlavor,
                    type = "Trinket",
                    slotType = equipment.itemDefinition.slotType.ToString(),
                    priceBuy = equipment.PriceBuy,
                    priceSell = equipment.PriceSell,
                    InventorySizeX = equipment.InventorySize.x,
                    InventorySizeY = equipment.InventorySize.y,
                    modifiersOnEquipNew = returnDTO
                });
                ImageHelpers.SaveBaseImage(equipment.itemDefinition, "Trinkets");
            }
            ClearInventory();
        }
    }
    [HarmonyPatch(typeof(InventoryItem), "Start")]
    public class AttachmentStatsInterceptor
    {
        static void Postfix(object __instance)
        {
            if (__instance == null) return;
            InventoryItem attachment = __instance as InventoryItem;
            
            if (attachment?.itemDefinition?.ItemType != ItemType.Attachment) return;
            if (attachment.itemDefinition.LocalizedDisplayName.StartsWith("Test")) return;
            if (attachment.itemDefinition.LocalizedDisplayName.Contains("Chamber Chisel")) return;

            Debug.Log("[ Mod: EverythingGrabber ] Postfix found an attachment");

            if (itHasBegun == false)
            {
                Debug.Log("[ Mod: EverythingGrabber ] Not ready for capture. Skipping...");
                return;
            }
            if (attachment == null)
            {
                Debug.Log("[ Mod: EverythingGrabber ] For some fucking reason attachment is null");
                return;
            }

            var helper = new ValueHelpers();
            AttachmentDTO returnDTO = AttachmentDTO.GetAttachmentDTO(attachment, helper);

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

            attachmentPropertyList.Add(returnDTO);
            ImageHelpers.SaveBaseImage(attachment.itemDefinition, "Attachments");
            
            ClearInventory();
        }
    }
    [HarmonyPatch(typeof(InventoryItem), "Start")]
    public class ChamberStatsInterceptor
    {
        static void Postfix(object __instance)
        {
            if (__instance == null) return;
            InventoryItem chamber = __instance as InventoryItem;
            
            if (!chamber.itemDefinition.LocalizedDisplayName.Contains("Chamber Chisel")) return;
            if (chamber.itemDefinition.LocalizedDisplayName.StartsWith("Test")) return;

            Debug.Log("[ Mod: EverythingGrabber ] Postfix found a chamber");

            if (itHasBegun == false)
            {
                Debug.Log("[ Mod: EverythingGrabber ] Not ready for capture. Skipping...");
                return;
            }
            if (chamber == null)
            {
                Debug.Log("[ Mod: EverythingGrabber ] For some fucking reason chamber is null");
                return;
            }

            var helper = new ValueHelpers();
            AttachmentDTO returnDTO = AttachmentDTO.GetAttachmentDTO(chamber, helper);

            if (returnDTO == null) 
            { 
                Debug.Log("[ Mod: EverythingGrabber ] ReturnDTO failed");
                return;
            }
            /*if (returnDTO.modifiers.Count == 0) 
            {
                Debug.Log("[ Mod: EverythingGrabber ] WHY WOULD MODIFIERS BE ZERO");
                return;
            }*/

            chamberPropertyList.Add(returnDTO);
            ImageHelpers.SaveBaseImage(chamber.itemDefinition, "Chambers");
            
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

        // Build weapon database list
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

        // Build enchantment database list
        foreach (var itemDef in itemDatabase)
        {
            if (itemDef?.ItemType != ItemType.Enchantment) continue;
            if (!itemDef.includedInEarlyAccess) continue;
            if (itemDef.LocalizedDisplayName.StartsWith("Test")) continue;

            enchantmentList.Add(itemDef);
        }

        // Build equipment database list
        foreach (var itemDef in itemDatabase)
        {
            if (!itemDef) continue;
            if (itemDef.ItemType != ItemType.Armor && itemDef.ItemType != ItemType.Misc) continue;
            if (!itemDef.includedInEarlyAccess) continue;
            if (itemDef.modifiersOnEquipNew.Count == 0) continue;

            itemDef?.alwaysSpawnWithFullDurability = true;
            equipmentList.Add(itemDef);
        }

        // Build attachment database list
        foreach (var itemDef in itemDatabase)
        {
            if (itemDef?.ItemType != ItemType.Attachment) continue;
            if (!itemDef.includedInEarlyAccess) continue;
            if (itemDef.LocalizedDisplayName.StartsWith("Test")) continue;
            if (itemDef.LocalizedDisplayName.Contains("Chamber Chisel")) continue;

            attachmentList.Add(itemDef);
        }

        // Build chamber database list
        foreach (var itemDef in itemDatabase)
        {
            if (!itemDef) continue;
            if (!itemDef.LocalizedDisplayName.Contains("Chamber Chisel")) continue;
            
            chamberList.Add(itemDef);
        }
        foreach (var chamber in chamberList)
        {
            Debug.Log($"[ Mod: EverythingGrabber ] Chamber list item: {chamber.LocalizedDisplayName}");
        }
        
    }
    private IEnumerator SpawnWeapons()
    {
        Debug.Log("[ Mod: EverythingGrabber ] Beginning weapon spawning & capture");
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
        Debug.Log("[ Mod: EverythingGrabber ] Weapon spawn & capture complete");
    }
    private IEnumerator SpawnEnchantments()
    {
        Debug.Log("[ Mod: EverythingGrabber ] Beginning enchantment spawning & capture");
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

        yield return WaitForListToSettle(() => oilList.Count + scrollList.Count);

        SaveData("Oils", oilList);
        SaveData("Scrolls", scrollList);
        itHasBegun = false;
        Debug.Log("[ Mod: EverythingGrabber ] Enchantment spawn & capture complete");
    }
    private IEnumerator SpawnAttachments()
    {
        Debug.Log("[ Mod: EverythingGrabber ] Beginning attachment spawning & capture");
        if (attachmentList.Count == 0) yield break;
        
        ClearSlots();
        ClearInventory();

        itHasBegun = true;

        foreach (var attachment in attachmentList)
        {
            InventorySlot slot = SpawnHelper.ToInventorySlot(attachment.slotType);
            StaticInstance<DevToolsManager>.Instance.SpawnToInventory(attachment);

            yield return null;
        }

        yield return WaitForListToSettle(() => attachmentPropertyList.Count);

        SaveData("Attachments", attachmentPropertyList);
        itHasBegun = false;
        Debug.Log("[ Mod: EverythingGrabber ] Attachment spawn & capture complete");
    }
    private IEnumerator SpawnChambers()
    {
        Debug.Log("[ Mod: EverythingGrabber ] Beginning chamber spawning & capture");
        if (chamberList.Count == 0) yield break;
        
        ClearSlots();
        ClearInventory();

        itHasBegun = true;

        foreach (var chamber in chamberList)
        {
            InventorySlot slot = SpawnHelper.ToInventorySlot(chamber.slotType);
            StaticInstance<DevToolsManager>.Instance.SpawnToInventory(chamber);

            yield return null;
        }

        yield return WaitForListToSettle(() => chamberPropertyList.Count);

        SaveData("Chambers", chamberPropertyList);
        itHasBegun = false;
        Debug.Log("[ Mod: EverythingGrabber ] Chamber spawn & capture complete");
    }
    private IEnumerator SpawnEquipment()
    {
        Debug.Log("[ Mod: EverythingGrabber ] Beginning equipment spawning & capture");
        if (equipmentList.Count == 0) yield break;
        
        ClearSlots();
        ClearInventory();

        itHasBegun = true;

        foreach (var equipment in equipmentList)
        {
            InventorySlot slot = SpawnHelper.ToInventorySlot(equipment.slotType);
            StaticInstance<DevToolsManager>.Instance.SpawnToInventory(equipment);

            yield return null;
        }

        yield return WaitForListToSettle(() => armorList.Count + trinketList.Count);

        SaveData("Armor", armorList);
        SaveData("Trinkets", trinketList);
        itHasBegun = false;
        Debug.Log("[ Mod: EverythingGrabber ] Equipment spawn & capture complete");
    }
    private static IEnumerator WaitForListToSettle(Func<int> getCount, int settleFrames = 60, int maxFrames = 1800)
    {
        int lastCount = -1;
        int stableFrames = 0;
        int totalFrames = 0;

        while (stableFrames < settleFrames && totalFrames < maxFrames)
        {
            int currentCount = getCount();
            if (currentCount == lastCount)
            {
                stableFrames++;
            }
            else
            {
                lastCount = currentCount;
                stableFrames = 0;
            }
            totalFrames++;
            yield return null;
        }
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

    private static void SaveData<T>(string type, List<T> items)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreUnchangedDefaultsResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(items, settings);
        string rootDir = Paths.GameRootPath;
        string folderPath = Path.Combine(rootDir, $"Extracted Data\\{type}\\");
        Directory.CreateDirectory(folderPath);
        string path = Path.Combine(folderPath, $"{type.ToLower()}.json");
        File.WriteAllText(path, json);

        string json2 = JsonConvert.SerializeObject(trinketList, settings);
        string rootDir2 = Paths.GameRootPath;
        string folderPath2 = Path.Combine(rootDir2, "Extracted Data\\Trinkets\\");
        Directory.CreateDirectory(folderPath2);
        string path2 = Path.Combine(folderPath2, "trinkets.json");
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