using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTask
{
    public Data.QuestTaskData TaskData {get; private set;}
    public int Count { get; set; }

    public QuestTask(Data.QuestTaskData questTaskData, int count)
	{
		TaskData = questTaskData;
		Count = count;
	}

	public bool IsCompleted()
	{
		if(TaskData.ObjectiveCount <= Count)
			return true;
		
		return false;
	}

    public void OnHandleBroadcastEvent(Define.EBroadcastEventType eventType, int value)
	{
		switch (TaskData.ObjectiveType)
		{
			case Define.EQuestObjectiveType.KillMonster:
				if (eventType == Define.EBroadcastEventType.KillMonster)
				{
					Count += value;
				}
				break;
			case Define.EQuestObjectiveType.EarnMeat:
			case Define.EQuestObjectiveType.SpendMeat:
				if (eventType == Define.EBroadcastEventType.ChangeMeat)
				{
					Count += value;
				}
				break;
			case Define.EQuestObjectiveType.EarnWood:
			case Define.EQuestObjectiveType.SpendWood:
				if (eventType == Define.EBroadcastEventType.ChangeWood)
				{
					Count += value;
				}
				break;
			case Define.EQuestObjectiveType.EarnMineral:
			case Define.EQuestObjectiveType.SpendMineral:
				if (eventType == Define.EBroadcastEventType.ChangeWood)
				{
					Count += value;
				}
				break;
			case Define.EQuestObjectiveType.EarnGold:
			case Define.EQuestObjectiveType.SpendGold:
				if (eventType == Define.EBroadcastEventType.ChangeGold)
				{
					Count += value;
				}
				break;
			case Define.EQuestObjectiveType.UseItem:
				break;
			case Define.EQuestObjectiveType.Survival:
				break;
			case Define.EQuestObjectiveType.ClearDungeon:
				if (eventType == Define.EBroadcastEventType.DungeonClear)
				{
					Count += value;
				}
				break;
		}
	}
}
