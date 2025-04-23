using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item 
{
    public enum ItemType
    {
        Shield,
        Tsunami,
        Rain,
        Thunder,
        Mouse,
        Exit
    }

    public static int GetCost(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Shield: return 30;
            case ItemType.Tsunami: return 40;
            case ItemType.Rain: return 20;
            case ItemType.Thunder: return 15;
            case ItemType.Mouse: return 30;
        }
        return 0;
    }

    public static string GetName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Shield: return "Shield";
            case ItemType.Tsunami: return "Tsunami";
            case ItemType.Rain: return "Rain";
            case ItemType.Thunder: return "Thunder";
            case ItemType.Mouse: return "Mouse";
            case ItemType.Exit: return "Exit";
        }
        return null;
    }

    public static string GetDescription(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Shield: return "PROTECT YOU FOR ONCE";
            case ItemType.Tsunami: return "DESTROY ALL TILES";
            case ItemType.Rain: return "SEED GROW FASTER";
            case ItemType.Thunder: return "DESTROY 15 TILES";
            case ItemType.Mouse: return "EAT ENEMY'S SEEDS";
            case ItemType.Exit: return "EXIT";
        }
        return null;
    }
    
}
