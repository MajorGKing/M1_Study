using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GameSaveData
{
	public int Wood = 0;
	public int Mineral = 0;
	public int Meat = 0;
	public int Gold = 0;

	public List<HeroSaveData> Heroes = new List<HeroSaveData>();

	public int ItemDbIdGenerator = 1;
	public List<ItemSaveData> Items= new List<ItemSaveData>();
	public List<QuestSaveData> AllQuests = new List<QuestSaveData>();
}

[Serializable]
public class HeroSaveData
{
	public int DataId = 0;
	public int Level = 1;
	public int Exp = 0;
	public HeroOwningState OwningState = HeroOwningState.Unowned;
}

public enum HeroOwningState
{
	Unowned,
	Owned,
	Picked,
}

[Serializable]
public class ItemSaveData
{
	public int InstanceId;
	public int DbId;
	public int TemplateId;
	public int Count;
	public int EquipSlot; // 장착 + 인벤 + 창고
	//public int OwnerId;
	public int EnchantCount;
}

[Serializable]
public class QuestSaveData
{
	public int TemplateId;
	public Define.EQuestState State = Define.EQuestState.None;
	public List<int> ProgressCount = new List<int>();
	public DateTime NextResetTime;
}

public class GameManager
{
	#region GameData
	GameSaveData _saveData = new GameSaveData();
	public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }

	public int Wood
	{
		get { return _saveData.Wood; }
		private set
		{
			_saveData.Wood = value;
			BroadcastEvent(Define.EBroadcastEventType.ChangeWood, value);
		}
	}

	public int Mineral
	{
		get { return _saveData.Mineral; }
		private set
		{
			_saveData.Mineral = value;
			BroadcastEvent(Define.EBroadcastEventType.ChangeMineral, value);
		}
	}

	public int Meat
	{
		get { return _saveData.Meat; }
		private set
		{
			_saveData.Meat = value;
			BroadcastEvent(Define.EBroadcastEventType.ChangeMeat, value);
		}
	}

	public int Gold
	{
		get { return _saveData.Gold; }
		private set
		{
			_saveData.Gold = value;
			BroadcastEvent(Define.EBroadcastEventType.ChangeGold, value);
		}
	}

	public void BroadcastEvent(Define.EBroadcastEventType eventType, int value)
	{
		OnBroadcastEvent?.Invoke(eventType, value);
	}

	public List<HeroSaveData> AllHeroes { get { return _saveData.Heroes; } }
	public int TotalHeroCount { get { return _saveData.Heroes.Count; } }
	public int UnownedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == HeroOwningState.Unowned).Count(); } }
	public int OwnedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == HeroOwningState.Owned).Count(); } }
	public int PickedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == HeroOwningState.Picked).Count(); } }

	public int GenerateItemDbId()
	{
		int itemDbId = _saveData.ItemDbIdGenerator;
		_saveData.ItemDbIdGenerator++;
		return itemDbId;
	}

	#endregion

	#region Hero
	private Vector2 _moveDir;
	public Vector2 MoveDir
	{
		get { return _moveDir; }
		set
		{
			_moveDir = value;
			OnMoveDirChanged?.Invoke(value);
		}
	}

	private Define.EJoystickState _joystickState;
	public Define.EJoystickState JoystickState
	{
		get { return _joystickState; }
		set
		{
			_joystickState = value;
			OnJoystickStateChanged?.Invoke(_joystickState);
		}
	}
	#endregion

	#region Teleport
	public void TeleportHeroes(Vector3 position)
	{
		TeleportHeroes(Managers.Map.World2Cell(position));
	}

	public void TeleportHeroes(Vector3Int cellPos)
	{
		foreach (var hero in Managers.Object.Heroes)
		{
			Vector3Int randCellPos = Managers.Game.GetNearbyPosition(hero, cellPos);
			Managers.Map.MoveTo(hero, randCellPos, forceMove: true);
		}

		Vector3 worldPos = Managers.Map.Cell2World(cellPos);
		Managers.Object.Camp.ForceMove(worldPos);
		Camera.main.transform.position = worldPos;
	}
	#endregion

	#region Helper
	public Vector3Int GetNearbyPosition(BaseObject hero, Vector3Int pivot, int range = 5)
	{
		int x = Random.Range(-range, range);
		int y = Random.Range(-range, range);

		for (int i = 0; i < 100; i++)
		{
			Vector3Int randCellPos = pivot + new Vector3Int(x, y, 0);
			if (Managers.Map.CanGo(hero, randCellPos))
				return randCellPos;
		}

		Debug.LogError($"GetNearbyPosition Failed");

		return Vector3Int.zero;
	}
	#endregion

	#region Save & Load
	public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }

	public void InitGame()
	{
		if (File.Exists(Path))
			return;

		var heroes = Managers.Data.HeroDic.Values.ToList();
		foreach (Data.HeroData hero in heroes)
		{
			HeroSaveData saveData = new HeroSaveData()
			{
				DataId = hero.DataId,
			};

			SaveData.Heroes.Add(saveData);
		}

		// Item
		{

		}

		// TEMP
		SaveData.Heroes[0].OwningState = HeroOwningState.Picked;
		SaveData.Heroes[1].OwningState = HeroOwningState.Owned;
	}

	public void SaveGame()
	{
		// Hero

		// Item
		{
			SaveData.Items.Clear();
			foreach(var item in Managers.Inventory.AllItems)
				SaveData.Items.Add(item.SaveData);
		}

		// Quest
		{
			// SaveData.ProcessingQuests.Clear();
			// SaveData.CompletedQuests.Clear();
			// SaveData.RewardedQuests.Clear();

			// foreach (Quest item in Managers.Quest.ProcessingQuests)
			// 	SaveData.ProcessingQuests.Add(item.SaveData);

			// foreach (Quest item in Managers.Quest.CompletedQuests)
			// 	SaveData.CompletedQuests.Add(item.SaveData);

			// foreach (Quest item in Managers.Quest.RewardedQuests)
			// 	SaveData.RewardedQuests.Add(item.SaveData);
		}

		string jsonStr = JsonUtility.ToJson(Managers.Game.SaveData);
		File.WriteAllText(Path, jsonStr);
		Debug.Log($"Save Game Completed : {Path}");
	}

	public bool LoadGame()
	{
		if (File.Exists(Path) == false)
			return false;

		string fileStr = File.ReadAllText(Path);
		GameSaveData data = JsonUtility.FromJson<GameSaveData>(fileStr);

		if (data != null)
			Managers.Game.SaveData = data;

		// Hero

		// Item
		{
			Managers.Inventory.Clear();

			foreach (ItemSaveData itemSaveData in data.Items)
			{
				Managers.Inventory.AddItem(itemSaveData);
			}
		}

		// Quest
		{
			// Managers.Quest.Clear(); 

			// foreach (QuestSaveData questSaveData in data.ProcessingQuests)
			// {
			// 	Managers.Quest.AddQuest(questSaveData);
			// }

			// foreach (QuestSaveData questSaveData in data.CompletedQuests)
			// {
			// 	Managers.Quest.AddQuest(questSaveData);
			// }

			// foreach (QuestSaveData questSaveData in data.RewardedQuests)
			// {
			// 	Managers.Quest.AddQuest(questSaveData);
			// }

			// Managers.Quest.AddUnknownQuests();
		}

		Debug.Log($"Save Game Loaded : {Path}");
		return true;
	}

	#endregion

	#region Action
	public event Action<Vector2> OnMoveDirChanged;
	public event Action<Define.EJoystickState> OnJoystickStateChanged;

	public event Action<Define.EBroadcastEventType, int> OnBroadcastEvent;
	#endregion
}
