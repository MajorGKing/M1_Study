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

	public int ItemDbIdGenerator = 1;
	public List<ItemSaveData> Items= new List<ItemSaveData>();
	public List<QuestSaveData> AllQuests = new List<QuestSaveData>();
	public List<HeroSaveData> Heroes = new List<HeroSaveData>();
}

[Serializable]
public class HeroSaveData
{
	public int DataId = 0;
	public int Level = 1;
	public int Exp = 0;
	public Define.EHeroOwningState OwningState = Define.EHeroOwningState.Unowned;
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
			int diff = _saveData.Wood - value;
			_saveData.Wood = value;
			OnBroadcastEvent?.Invoke(Define.EBroadcastEventType.ChangeWood, diff);
		}
	}

	public int Mineral
	{
		get { return _saveData.Mineral; }
		private set
		{
			int diff = _saveData.Mineral - value;
			_saveData.Mineral = value;
			OnBroadcastEvent?.Invoke(Define.EBroadcastEventType.ChangeMineral, diff);
		}
	}

	public int Meat
	{
		get { return _saveData.Meat; }
		private set
		{
			int diff = _saveData.Meat - value;
			_saveData.Meat = value;
			OnBroadcastEvent?.Invoke(Define.EBroadcastEventType.ChangeMeat, diff);
		}
	}

	public int Gold
	{
		get { return _saveData.Gold; }
		private set
		{
			int diff = _saveData.Gold - value;
			_saveData.Gold = value;
			OnBroadcastEvent?.Invoke(Define.EBroadcastEventType.ChangeGold, diff);
		}
	}

	public bool CheckResource(Define.EResourceType eResourceType, int amount)
	{
		switch (eResourceType)
		{
			case Define.EResourceType.Wood:
				return Wood >= amount;
			case Define.EResourceType.Mineral:
				return Mineral >= amount;
			case Define.EResourceType.Meat:
				return Meat >= amount;
			case Define.EResourceType.Gold:
				return Gold >= amount;
			case Define.EResourceType.Dia:
				return true;
			case Define.EResourceType.Materials:
				return true;
			default:
				return false;
		}
	}

	public bool SpendResource(Define.EResourceType eResourceType, int amount)
	{
		if (CheckResource(eResourceType, amount) == false)
			return false;

		switch (eResourceType)
		{
			case Define.EResourceType.Wood:
				Wood -= amount;
				break;
			case Define.EResourceType.Mineral:
				Mineral -= amount;
				break;
			case Define.EResourceType.Meat:
				Meat -= amount;
				break;
			case Define.EResourceType.Gold:
				Gold -= amount;
				break;
			case Define.EResourceType.Dia:
				break;
			case Define.EResourceType.Materials:
				break;
		}

		return true;
	}

	public void EarnResource(Define.EResourceType eResourceType, int amount)
	{
		switch (eResourceType)
		{
			case Define.EResourceType.Wood:
				Wood += amount;
				break;
			case Define.EResourceType.Mineral:
				Mineral += amount;
				break;
			case Define.EResourceType.Meat:
				Meat += amount;
				break;
			case Define.EResourceType.Gold:
				Gold += amount;
				break;
			case Define.EResourceType.Dia:
				break;
			case Define.EResourceType.Materials:
				break;
		}
	}



	public void BroadcastEvent(Define.EBroadcastEventType eventType, int value)
	{
		OnBroadcastEvent?.Invoke(eventType, value);
	}

	public List<HeroSaveData> AllHeroes { get { return _saveData.Heroes; } }
	public int TotalHeroCount { get { return _saveData.Heroes.Count; } }
	public int UnownedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == Define.EHeroOwningState.Unowned).Count(); } }
	public int OwnedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == Define.EHeroOwningState.Owned).Count(); } }
	public int PickedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == Define.EHeroOwningState.Picked).Count(); } }

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

		// Hero
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

		// Quest
		{
			var quests = Managers.Data.QuestDic.Values.ToList();

			foreach (Data.QuestData questData in quests)
			{
				QuestSaveData saveData = new QuestSaveData()
				{
					TemplateId = questData.DataId,
					State = Define.EQuestState.None,
					ProgressCount = new List<int>(),
					NextResetTime = DateTime.Now,
				};

				for (int i = 0; i < questData.QuestTasks.Count; i++)
				{
					saveData.ProgressCount.Add(0);
				}

				Debug.Log("SaveDataQuest");
				Managers.Quest.AddQuest(saveData);
			}
		}

		// TEMP
		SaveData.Heroes[0].OwningState = Define.EHeroOwningState.Picked;
		SaveData.Heroes[1].OwningState = Define.EHeroOwningState.Owned;

		Wood = 100;
		Gold = 100;
		Mineral = 100;
		Meat = 100;
	}

	public void SaveGame()
	{
		// Hero
		{
			SaveData.Heroes.Clear();
			foreach (var heroinfo in Managers.Hero.AllHeroInfos.Values)
			{
				SaveData.Heroes.Add(heroinfo.SaveData);
			}
		}

		// Item
		{
			SaveData.Items.Clear();
			foreach(var item in Managers.Inventory.AllItems)
				SaveData.Items.Add(item.SaveData);
		}

		// Quest
		{
			SaveData.AllQuests.Clear();
			foreach (Quest quest in Managers.Quest.AllQuests.Values)
			{
				SaveData.AllQuests.Add(quest.SaveData);
			}
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
		{
			Managers.Hero.AllHeroInfos.Clear();

			foreach (var saveData in data.Heroes)
			{
				Managers.Hero.AddHeroInfo(saveData);
			}

			Managers.Hero.AddUnknownHeroes();
		}

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
			Managers.Quest.Clear();

			foreach (QuestSaveData questSaveData in data.AllQuests)
			{
				Managers.Quest.AddQuest(questSaveData);
			}

			Managers.Quest.AddUnknownQuests();
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
