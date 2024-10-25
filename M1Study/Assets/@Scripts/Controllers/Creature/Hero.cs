using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Creature
{
	public bool NeedArrange {get;set;}
	public override Define.ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value)
			{
				base.CreatureState = value;
			}
		}
	}

	Define.EHeroMoveState _heroMoveState = Define.EHeroMoveState.None;
	public Define.EHeroMoveState HeroMoveState
	{
		get { return _heroMoveState; }
		private set
		{
			_heroMoveState = value;
			switch (value)
			{
				case Define.EHeroMoveState.CollectEnv:
					NeedArrange = true;
					break;
				case Define.EHeroMoveState.TargetMonster:
					NeedArrange = true;
					break;
				case Define.EHeroMoveState.ForceMove:
					NeedArrange = true;
					break;
			}
		}
	}

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = Define.ECreatureType.Hero;

		Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
		Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

		// Map
		Collider.isTrigger = true;
		RigidBody.simulated = false;

		StartCoroutine(CoUpdateAI());

		return true;
	}

	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		// State
		CreatureState = Define.ECreatureState.Idle;

		// Skill
		Skills = gameObject.GetOrAddComponent<SkillComponent>();
		Skills.SetInfo(this, CreatureData.SkillIdList);
	}

	#region AI
	public float AttackDistance
	{
		get
		{
			float targetRadius = (Target.IsValid() ? Target.ColliderRadius : 0);
			return ColliderRadius + targetRadius + 2.0f;
		}
	}

	public float StopDistance { get; private set; } = 1.0f;
	
	public Transform HeroCampDest
    {
        get
        {
			HeroCamp camp = Managers.Object.Camp;
			if (HeroMoveState == Define.EHeroMoveState.ReturnToCamp)
				return camp.Pivot;

			return camp.Destination;
        }
    }
	
	protected override void UpdateIdle() 
	{
		// 0. 이동 상태라면 강제 변경
		if (HeroMoveState == Define.EHeroMoveState.ForceMove)
		{
			CreatureState = Define.ECreatureState.Move;
			return;
		}

        // 0. 너무 멀어졌다면 강제로 이동

        // 1. 몬스터
        Creature creature = FindClosestInRange(Define.HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
        if (creature != null)
        {
            Target = creature;
            CreatureState = Define.ECreatureState.Move;
            HeroMoveState = Define.EHeroMoveState.TargetMonster;
            return;
        }

		// 2. 주변 Env 채굴
		Env env = FindClosestInRange(Define.HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Env;
		if (env != null)
		{
			Target = env;
			CreatureState = Define.ECreatureState.Move;
			HeroMoveState = Define.EHeroMoveState.CollectEnv;
			return;
		}

		// 3. Camp 주변으로 모이기
		if (NeedArrange)
		{
			CreatureState = Define.ECreatureState.Move;
			HeroMoveState = Define.EHeroMoveState.ReturnToCamp;
			return;
		}
	}
	protected override void UpdateMove() 
	{
		// 0. 누르고 있다면, 강제 이동
		if (HeroMoveState == Define.EHeroMoveState.ForceMove)
		{
			Define.EFindPathResult result = FindPathAndMoveToCellPos(HeroCampDest.position, Define.HERO_DEFAULT_MOVE_DEPTH);
			return;
		}

		// 1. 주변 몬스터 서치
		if (HeroMoveState == Define.EHeroMoveState.TargetMonster)
		{
			// 몬스터 죽었으면 포기.
			if (Target.IsValid() == false)
			{
				HeroMoveState = Define.EHeroMoveState.None;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			SkillBase skill = Skills.GetReadySkill();
			ChaseOrAttackTarget(Define.HERO_SEARCH_DISTANCE, skill);
			//ChaseOrAttackTarget(AttackDistance, Define.HERO_SEARCH_DISTANCE);
			return;
		}

		// 2. 주변 Env 채굴
		if (HeroMoveState == Define.EHeroMoveState.CollectEnv)
		{
			// 몬스터가 있으면 포기.
			Creature creature = FindClosestInRange(Define.HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
			if (creature != null)
			{
				Target = creature;
				HeroMoveState = Define.EHeroMoveState.TargetMonster;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			// Env 이미 채집했으면 포기.
			if (Target.IsValid() == false)
			{
				HeroMoveState = Define.EHeroMoveState.None;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			SkillBase skill = Skills.GetReadySkill();
			ChaseOrAttackTarget(Define.HERO_SEARCH_DISTANCE, skill);
			return;
		}

		// 3. Camp 주변으로 모이기
		if (HeroMoveState == Define.EHeroMoveState.ReturnToCamp)
		{
			Vector3 destPos = HeroCampDest.position;
			if (FindPathAndMoveToCellPos(destPos, Define.HERO_DEFAULT_MOVE_DEPTH) == Define.EFindPathResult.Success)
				return;

			// 실패 사유 검사.
			BaseObject obj = Managers.Map.GetObject(destPos);
			if (obj.IsValid())
			{
				// 내가 그 자리를 차지하고 있다면
				if (obj == this)
				{
					HeroMoveState = Define.EHeroMoveState.None;
					NeedArrange = false;
					return;
				}

				// 다른 영웅이 멈춰있다면.
				Hero hero = obj as Hero;
				if (hero != null && hero.CreatureState == Define.ECreatureState.Idle)
				{
					HeroMoveState = Define.EHeroMoveState.None;
					NeedArrange = false;
					return;
				}
			}
		}

		// 4. 기타 (누르다 뗐을 때)
		if (LerpCellPosCompleted)
			CreatureState = Define.ECreatureState.Idle;
	}
	protected override void UpdateSkill() 
	{
		if (HeroMoveState == Define.EHeroMoveState.ForceMove)
		{
			CreatureState = Define.ECreatureState.Move;
			return;
		}

		if (Target.IsValid() == false)
		{
			CreatureState = Define.ECreatureState.Move;
			return;
		}
	}
	protected override void UpdateDead() 
	{
		
	}

	
	#endregion
	private void HandleOnJoystickStateChanged(Define.EJoystickState joystickState)
	{
		switch (joystickState)
		{
			case Define.EJoystickState.PointerDown:
				HeroMoveState = Define.EHeroMoveState.ForceMove;
				break;
			case Define.EJoystickState.Drag:
				HeroMoveState = Define.EHeroMoveState.ForceMove;
				break;
			case Define.EJoystickState.PointerUp:
				HeroMoveState = Define.EHeroMoveState.None;
				break;
			default:
				break;
		}
	}

	public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
	{
		base.OnAnimEventHandler(trackEntry, e);
	}
}