using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public ItemSaveData SaveData { get; set; }
    public int InstanceId
	{
		get { return SaveData.InstanceId; }
		set { SaveData.InstanceId = value; }
	}

	public long DbId
	{
		get { return SaveData.DbId; }
	}

	public int TemplateId
	{
		get { return SaveData.TemplateId; }
		set { SaveData.TemplateId = value; }
	}

	public int Count
	{
		get { return SaveData.Count; }
		set { SaveData.Count = value; }
	}

	public int EquipSlot
	{
		get { return SaveData.EquipSlot; }
		set { SaveData.EquipSlot = value; }
	}

    public Data.ItemData TemplateData
    {
        get
        {
            return Managers.Data.ItemDic[TemplateId];
        }
    }

    public Define.EItemType ItemType { get; private set; }
	public Define.EItemSubType SubType { get; private set; }

    public Item(int templateId)
	{
		TemplateId = templateId;
		ItemType = TemplateData.Type;
		SubType = TemplateData.SubType;
	}

    public virtual bool Init()
	{
		return true;
	}

    public static Item MakeItem(ItemSaveData itemInfo)
    {
        if (Managers.Data.ItemDic.TryGetValue(itemInfo.TemplateId, out Data.ItemData itemData) == false)
			return null;

        Item item = null;

        switch (itemData.Type)
		{
			case Define.EItemType.Weapon:
				item = new Equipment(itemInfo.TemplateId);
				break;
			case Define.EItemType.Armor:
				item = new Equipment(itemInfo.TemplateId);
				break;
			case Define.EItemType.Potion:
				item = new Consumable(itemInfo.TemplateId);
				break;
			case Define.EItemType.Scroll:
				item = new Consumable(itemInfo.TemplateId);
				break;
		}

        if (item != null)
		{
			item.SaveData = itemInfo;
			item.InstanceId = itemInfo.InstanceId;
			item.Count = itemInfo.Count;
		}

        return item;
    }

    	#region Helpers
	public bool IsEquippable()
	{
		return GetEquipItemEquipSlot() != Define.EEquipSlotType.None;
	}

	public Define.EEquipSlotType GetEquipItemEquipSlot()
	{
		if (ItemType == Define.EItemType.Weapon)
			return Define.EEquipSlotType.Weapon;

		if (ItemType == Define.EItemType.Armor)
		{
			switch (SubType)
			{
				case Define.EItemSubType.Helmet:
					return Define.EEquipSlotType.Helmet;
				case Define.EItemSubType.Armor:
					return Define.EEquipSlotType.Armor;
				case Define.EItemSubType.Shield:
					return Define.EEquipSlotType.Shield;
				case Define.EItemSubType.Gloves:
					return Define.EEquipSlotType.Gloves;
				case Define.EItemSubType.Shoes:
					return Define.EEquipSlotType.Shoes;
			}
		}

		return Define.EEquipSlotType.None;
	}

	public bool IsEquippedItem()
	{
		return SaveData.EquipSlot > (int)Define.EEquipSlotType.None && SaveData.EquipSlot < (int)Define.EEquipSlotType.EquipMax;
	}

	public bool IsInInventory()
	{
		return SaveData.EquipSlot == (int)Define.EEquipSlotType.Inventory;
	}

	public bool IsInWarehouse()
	{
		return SaveData.EquipSlot == (int)Define.EEquipSlotType.WareHouse;
	}
	#endregion
}

public class Equipment : Item
{
    public int Damage { get; private set; }
	public int Defence { get; private set; }
	public double Speed { get; private set; }

    protected Data.EquipmentData EquipmentData { get { return (Data.EquipmentData)TemplateData; } }

    public Equipment(int templateId) : base(templateId)
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
			return false;

		if (TemplateData == null)
			return false;

		if (TemplateData.Type != Define.EItemType.Armor || TemplateData.Type != Define.EItemType.Weapon)
			return false;

		Data.EquipmentData data = (Data.EquipmentData)TemplateData;
		{
			Damage = data.Damage;
			Defence = data.Defence;
			Speed = data.Speed;
		}

		return true;
    }
}

public class Consumable : Item
{
    public double Value { get; private set; }

    public Consumable(int templateId) : base(templateId)
    {
        Init();
    }

    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		if (TemplateData == null)
			return false;

		if (TemplateData.Type != Define.EItemType.Potion || TemplateData.Type != Define.EItemType.Scroll)
			return false;

		Data.ConsumableData data = (Data.ConsumableData)TemplateData;
		{
			Value = data.Value;
		}

		return true;
	}
}
