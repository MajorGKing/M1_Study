using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class Quest
{
    public QuestSaveData SaveData { get; set; }
    private QuestData QuestData;
    public List<QuestTask> _questTasks = new List<QuestTask>();

    public int TemplateId
	{
		get { return SaveData.TemplateId; }
		set { SaveData.TemplateId = value; }
	}

	public Define.EQuestState State
	{
		get { return SaveData.State; }
		set { SaveData.State = value; }
	}

	public QuestTask GetCurrentTask()
	{
		foreach (QuestTask task in _questTasks)
		{
			if (task.IsCompleted() == false)
				return task;
		}

		return null;
	}

	public bool IsCompleted()
	{
		for (int i = 0; i < QuestData.QuestTasks.Count; i++)
		{
			if (i >= SaveData.ProgressCount.Count)
				return false;

			QuestTaskData questTaskData = QuestData.QuestTasks[i];

			int progressCount = SaveData.ProgressCount[i];
			if (progressCount < questTaskData.ObjectiveCount)
				return false;
		}

		return true;
	}

	public void GiveReward()
	{
		if (SaveData.State == Define.EQuestState.Rewarded)
			return;

		if (IsCompleted() == false)
			return;

		SaveData.State = Define.EQuestState.Rewarded;

		foreach (var reward in QuestData.Rewards)
		{
			switch (reward.RewardType)
			{
				case Define.EQuestRewardType.Gold:
					Managers.Game.EarnResource(Define.EResourceType.Gold, reward.RewardCount);
					break;
				case Define.EQuestRewardType.Hero:
					int heroId = reward.RewardDataId;
					Managers.Hero.AcquireHeroCard(heroId, reward.RewardCount);
					Managers.Hero.PickHero(heroId, Vector3Int.zero);
					break;
				case Define.EQuestRewardType.Meat:
					Managers.Game.EarnResource(Define.EResourceType.Meat, reward.RewardCount);
					break;
				case Define.EQuestRewardType.Mineral:
					Managers.Game.EarnResource(Define.EResourceType.Mineral, reward.RewardCount);
					break;
				case Define.EQuestRewardType.Wood:
					Managers.Game.EarnResource(Define.EResourceType.Wood, reward.RewardCount);
					break;
				case Define.EQuestRewardType.Item:
					break;
			}
		}
	}

    public Quest(QuestSaveData saveData)
    {
		SaveData = saveData;
		State = Define.EQuestState.None;

        QuestData = Managers.Data.QuestDic[TemplateId];

        _questTasks.Clear();

        for (int i = 0; i < QuestData.QuestTasks.Count; i++)
		{
			_questTasks.Add(new QuestTask(QuestData.QuestTasks[i], saveData.ProgressCount[i]));
		}
    }

    public static Quest MakeQuest(QuestSaveData saveData)
    {
        if (Managers.Data.QuestDic.TryGetValue(saveData.TemplateId, out QuestData questData) == false)
			return null;

		Quest quest = new Quest(saveData);

		if (quest != null)
		{
			quest.SaveData = saveData;
		}

		return quest;
    }

    public void OnHandleBroadcastEvent(Define.EBroadcastEventType eventType, int value)
	{
		if (eventType == Define.EBroadcastEventType.QuestClear)
			return;

		GetCurrentTask().OnHandleBroadcastEvent(eventType, value);

		for (int i = 0; i < _questTasks.Count; i++)
		{
			SaveData.ProgressCount[i] = _questTasks[i].Count;
		}

		if (IsCompleted() && State != Define.EQuestState.Rewarded)
		{
			State = Define.EQuestState.Completed;
			GiveReward(); // Rewarded State
            Managers.Game.BroadcastEvent(Define.EBroadcastEventType.QuestClear, QuestData.DataId);
		}

	}
}