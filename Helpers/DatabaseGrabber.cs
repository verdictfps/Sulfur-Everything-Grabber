using System;
using System.Collections.Generic;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;

class DatabaseGrabber
{
    public List<ItemDefinition> GetListOfItemDefinitions()
    {
        var database = StaticInstance<AsyncAssetLoading>.Instance.itemDatabase.GetRawList();
        if (database == null)
        {
            throw new ArgumentNullException(nameof(database));
        }

        return database;
    }

    public static CaliberType[] GetCaliberDatabase()
    {
        var Caliberdatabase = StaticInstance<AsyncAssetLoading>.Instance.assetSets.caliberTypes;
        if (Caliberdatabase == null)
        {
            throw new ArgumentNullException(nameof(Caliberdatabase));
        }

        return Caliberdatabase;
    }

    public static CaliberType GetCaliberEntry(WeaponSO weaponSO)
    {
        if (weaponSO == null)
        {
            throw new ArgumentNullException(nameof(weaponSO));
        }

        var caliberDatabase = GetCaliberDatabase();
        var index = (int)weaponSO.caliber;

        if (index < 0 || index >= caliberDatabase.Length)
        {
            throw new IndexOutOfRangeException($"Caliber index {index} is out of range.");
        }

        return caliberDatabase[index];
    }

}