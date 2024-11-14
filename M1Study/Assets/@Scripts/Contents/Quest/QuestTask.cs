using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTask
{
    public Data.QuestTaskData _questTaskData;
    public int Count { get; set; }

    public QuestTask(Data.QuestTaskData questTaskData)
	{
		_questTaskData = questTaskData;
	}

	public bool IsCompleted()
	{
		// TODO
		return false;
	}

    public void OnHandleBroadcastEvent(Define.EBroadcastEventType eventType, int value)
	{
		// _questTaskData.ObjectiveType와 eventType 비교
	}
}
