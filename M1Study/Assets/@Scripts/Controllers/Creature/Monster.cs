using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Creature
{
    public override Define.ECreatureState CreatureState 
	{
		get { return base.CreatureState; }
		set
		{
			if (_creatureState != value)
			{
				base.CreatureState = value;
				switch (value)
				{
					case Define.ECreatureState.Idle:
						UpdateAITick = 0.5f;
						break;
					case Define.ECreatureState.Move:
						UpdateAITick = 0.0f;
						break;
					case Define.ECreatureState.Skill:
						UpdateAITick = 0.0f;
						break;
					case Define.ECreatureState.Dead:
						UpdateAITick = 1.0f;
						break;
				}
			}
		}
	}

    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = Define.ECreatureType.Monster;

		StartCoroutine(CoUpdateAI());

		return true;
	}

	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		// State
		CreatureState = Define.ECreatureState.Idle;		
	}

	private void Start() 
    {
        _initPos = transform.position;
    }

    #region AI
	Vector3 _destPos;
	Vector3 _initPos;

    protected override void UpdateIdle()
	{
		// Patrol
		{
			int patrolPercent = 10;
			int rand = Random.Range(0, 100);
			if (rand <= patrolPercent)
			{
				_destPos = _initPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2));
				CreatureState = Define.ECreatureState.Move;
				return;
			}
		}

		// Search Player
		Creature creature = FindClosestInRange(Define.MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
		if (creature != null)
		{
			Target = creature;
			CreatureState = Define.ECreatureState.Move;
			return;
		}
		
	}

	protected override void UpdateMove()
	{
		if (Target.IsValid() == false)
		{
			Creature creature = FindClosestInRange(Define.MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
			if (creature != null)
			{
				Target = creature;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			// Move
			FindPathAndMoveToCellPos(_destPos, Define.MONSTER_DEFAULT_MOVE_DEPTH);

			if (LerpCellPosCompleted)
			{
				CreatureState = Define.ECreatureState.Idle;
				return;
			}
		}
		else
		{
			// Chase
			ChaseOrAttackTarget(Define.MONSTER_SEARCH_DISTANCE, AttackDistance);

			// 너무 멀어지면 포기.
			if (Target.IsValid() == false)
			{
				Target = null;
				_destPos = _initPos;
				return;
			}
		}
	}

	protected override void UpdateSkill()
	{
		base.UpdateSkill();

		if (Target.IsValid() == false)
		{
			Target = null;
			_destPos = _initPos;
			CreatureState = Define.ECreatureState.Move;
			return;
		}
	}

	protected override void UpdateDead()
	{

	}
	#endregion

	#region Battle
	public override void OnDamaged(BaseObject attacker, SkillBase skill)
	{
		base.OnDamaged(attacker, skill);
	}

	public override void OnDead(BaseObject attacker, SkillBase skill)
	{
		base.OnDead(attacker, skill);

		// TODO : Drop Item

		Managers.Object.Despawn(this);
	}
	#endregion
}
