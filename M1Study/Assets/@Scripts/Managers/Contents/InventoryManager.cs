using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager
{
    public readonly int DEFAULT_INVENTORY_SLOT_COUNT = 30;
    public List<Item> AllItems { get; } = new List<Item>();

    // Cache
	Dictionary<int /*EquipSlot*/, Item> EquippedItems = new Dictionary<int, Item>(); // 장비 인벤
	List<Item> InventoryItems = new List<Item>(); // 가방 인벤
	List<Item> WarehouseItems = new List<Item>(); // 창고

    public Item MakeItem(int itemTemplateId, int count = 1)
    {
        int itemDbId = Managers.Game.GenerateItemDbId();

        if (Managers.Data.ItemDic.TryGetValue(itemTemplateId, out Data.ItemData itemData) == false)
			return null;

        ItemSaveData saveData = new ItemSaveData()
		{
			InstanceId = itemDbId,
			DbId = itemDbId,
			TemplateId = itemTemplateId,
			Count = count,
			EquipSlot = (int)Define.EEquipSlotType.Inventory,
			EnchantCount = 0,
		};

		return AddItem(saveData);
    }

    public Item AddItem(ItemSaveData itemInfo)
    {
        Item item = Item.MakeItem(itemInfo);
		if (item == null)
			return null;

        if (item.IsEquippedItem())
		{
			EquippedItems.Add(item.SaveData.EquipSlot, item);
		}
		else if (item.IsInInventory())
		{
			InventoryItems.Add(item);
		}
		else if (item.IsInWarehouse())
		{
			WarehouseItems.Add(item);
		}

		AllItems.Add(item);

		return item;
    }

    public void RemoveItem(int instanceId)
	{
		Item item = AllItems.Find(x => x.SaveData.InstanceId == instanceId);
		if (item == null)
			return;

		if (item.IsEquippedItem())
		{
			EquippedItems.Remove(item.SaveData.EquipSlot);
		}
		else if (item.IsInInventory())
		{
			InventoryItems.Remove(item);
		}
		else if (item.IsInWarehouse())
		{
			WarehouseItems.Remove(item);
		}

		AllItems.Remove(item);
	}

    public void EquipItem(int instanceId)
	{
		Item item = InventoryItems.Find(x => x.SaveData.InstanceId == instanceId);
		if (item == null)
		{
			Debug.Log("아이템존재안함");
			return;
		}

		Define.EEquipSlotType equipSlotType = item.GetEquipItemEquipSlot();
		if (equipSlotType == Define.EEquipSlotType.None)
			return;

		// 기존 아이템 해제
		if (EquippedItems.TryGetValue((int)equipSlotType, out Item prev))
			UnEquipItem(prev.InstanceId);

		// 아이템 장착
		item.EquipSlot = (int)equipSlotType;
		EquippedItems[(int)equipSlotType] = item;

        // TODO UI구독 부르기
	}

    public void UnEquipItem(int instanceId, bool checkFull = true)
	{
		var item = EquippedItems.Values.Where(x => x.InstanceId == instanceId).FirstOrDefault();
		if (item == null)
			return;

		// TODO 체크

		if (checkFull && IsInventoryFull())
			return;

		EquippedItems.Remove((int)item.EquipSlot);

		item.EquipSlot = (int)Define.EEquipSlotType.Inventory;
		InventoryItems.Add(item);
	}

    public void Clear()
	{
		AllItems.Clear();

		EquippedItems.Clear();
		InventoryItems.Clear();
		WarehouseItems.Clear();
	}

    #region Helper
	public Item GetItem(int instanceId)
	{
		return AllItems.Find(item => item.InstanceId == instanceId);
	}

	public Item GetEquippedItem(Define.EEquipSlotType equipSlotType)
	{
		EquippedItems.TryGetValue((int)equipSlotType, out Item item);

		return item;
	}

	public Item GetEquippedItem(int instanceId)
	{
		return EquippedItems.Values.Where(x => x.InstanceId == instanceId).FirstOrDefault();
	}

	public Item GetEquippedItemBySubType(Define.EItemSubType subType)
	{
		return EquippedItems.Values.Where(x => x.SubType == subType).FirstOrDefault();
	}

	public Item GetItemInInventory(int instanceId)
	{
		return InventoryItems.Find(x => x.SaveData.InstanceId == instanceId);
	}

	public bool IsInventoryFull()
	{
		return InventoryItems.Count >= InventorySlotCount();
	}

	public int InventorySlotCount()
	{
		return DEFAULT_INVENTORY_SLOT_COUNT;
	}

	public List<Item> GetEquippedItems()
	{
		return EquippedItems.Values.ToList();
	}

	public List<ItemSaveData> GetEquippedItemInfos()
	{
		return EquippedItems.Values.Select(x => x.SaveData).ToList();
	}

	public List<Item> GetInventoryItems()
	{
		return InventoryItems.ToList();
	}

	public List<ItemSaveData> GetInventoryItemInfos()
	{
		return InventoryItems.Select(x => x.SaveData).ToList();
	}

	public List<ItemSaveData> GetInventoryItemInfosOrderbyGrade()
	{
		return InventoryItems.OrderByDescending(y => (int)y.TemplateData.Grade)
						.ThenBy(y => (int)y.TemplateId)
						.Select(x => x.SaveData)
						.ToList();
	}

	public List<ItemSaveData> GetWarehouseItemInfos()
	{
		return WarehouseItems.Select(x => x.SaveData).ToList();
	}

	public List<ItemSaveData> GetWarehouseItemInfosOrderbyGrade()
	{
		return WarehouseItems.OrderByDescending(y => (int)y.TemplateData.Grade)
									.ThenBy(y => (int)y.TemplateId)
									.Select(x => x.SaveData)
									.ToList();
	}
	#endregion
}
