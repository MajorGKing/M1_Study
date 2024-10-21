using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Creature
{
	bool _needArrange = true;
	public bool NeedArrange
	{
		get { return _needArrange; }
		set
		{
			_needArrange = value;

			if (value)
				ChangeColliderSize(Define.EColliderSize.Big);
			else
				TryResizeCollider();
		}
	}
	public override Define.ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value)
			{
				base.CreatureState = value;

				if (value == Define.ECreatureState.Move)
					RigidBody.mass = CreatureData.Mass;
				else
					RigidBody.mass = CreatureData.Mass * 0.1f;
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

		StartCoroutine(CoUpdateAI());

		return true;
	}

	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		// State
		CreatureState = Define.ECreatureState.Idle;
	}

	#region AI
	public float SearchDistance { get; private set; } = 8.0f;
	public float AttackDistance
	{
		get
		{
			float targetRadius = (_target.IsValid() ? _target.ColliderRadius : 0);
			return ColliderRadius + targetRadius + 2.0f;
		}
	}

	public float StopDistance { get; private set; } = 1.0f;
	private BaseObject _target;
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
        Creature creature = FindClosestInRange(SearchDistance, Managers.Object.Monsters) as Creature;
        if (creature != null)
        {
            _target = creature;
            CreatureState = Define.ECreatureState.Move;
            HeroMoveState = Define.EHeroMoveState.TargetMonster;
            return;
        }

		// 2. 주변 Env 채굴
		Env env = FindClosestInRange(SearchDistance, Managers.Object.Envs) as Env;
		if (env != null)
		{
			_target = env;
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
			Vector3 dir = HeroCampDest.position - transform.position;
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);
			return;
		}

		// 1. 주변 몬스터 서치
		if (HeroMoveState == Define.EHeroMoveState.TargetMonster)
		{
			// 몬스터 죽었으면 포기.
			if (_target.IsValid() == false)
			{
				HeroMoveState = Define.EHeroMoveState.None;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			ChaseOrAttackTarget(AttackDistance, SearchDistance);
			return;
		}

		// 2. 주변 Env 채굴
		if (HeroMoveState == Define.EHeroMoveState.CollectEnv)
		{
			// 몬스터가 있으면 포기.
			Creature creature = FindClosestInRange(SearchDistance, Managers.Object.Monsters) as Creature;
			if (creature != null)
			{
				_target = creature;
				HeroMoveState = Define.EHeroMoveState.TargetMonster;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			// Env 이미 채집했으면 포기.
			if (_target.IsValid() == false)
			{
				HeroMoveState = Define.EHeroMoveState.None;
				CreatureState = Define.ECreatureState.Move;
				return;
			}

			ChaseOrAttackTarget(AttackDistance, SearchDistance);
			return;
		}

		// 3. Camp 주변으로 모이기
		if (HeroMoveState == Define.EHeroMoveState.ReturnToCamp)
		{
			Vector3 dir = HeroCampDest.position - transform.position;
			float stopDistanceSqr = StopDistance * StopDistance;
			if (dir.sqrMagnitude <= StopDistance)
			{
				HeroMoveState = Define.EHeroMoveState.None;
				CreatureState = Define.ECreatureState.Idle;
				NeedArrange = false;
				return;
			}
			else
			{
				// 멀리 있을 수록 빨라짐
				float ratio = Mathf.Min(1, dir.magnitude); // TEMP
				float moveSpeed = MoveSpeed * (float)Math.Pow(ratio, 3);
				SetRigidBodyVelocity(dir.normalized * moveSpeed);
				return;
			}
		}


		// 4. 기타 (누르다 뗐을 때)
		CreatureState = Define.ECreatureState.Idle;
	}
	protected override void UpdateSkill() 
	{
		if (HeroMoveState == Define.EHeroMoveState.ForceMove)
		{
			CreatureState = Define.ECreatureState.Move;
			return;
		}

		if (_target.IsValid() == false)
		{
			CreatureState = Define.ECreatureState.Move;
			return;
		}
	}
	protected override void UpdateDead() { }

	BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs)
	{
		BaseObject target = null;
		float bestDistanceSqr = float.MaxValue;
		float searchDistanceSqr = range * range;

		foreach (BaseObject obj in objs)
		{
			Vector3 dir = obj.transform.position - transform.position;
			float distToTargetSqr = dir.sqrMagnitude;

			// 서치 범위보다 멀리 있으면 스킵.
			if (distToTargetSqr > searchDistanceSqr)
				continue;

			// 이미 더 좋은 후보를 찾았으면 스킵.
			if (distToTargetSqr > bestDistanceSqr)
				continue;

			target = obj;
			bestDistanceSqr = distToTargetSqr;
		}

		return target;
	}

	void ChaseOrAttackTarget(float attackRange, float chaseRange)
	{
		Vector3 dir = (_target.transform.position - transform.position);
		float distToTargetSqr = dir.sqrMagnitude;
		float attackDistanceSqr = attackRange * attackRange;

		if (distToTargetSqr <= attackDistanceSqr)
		{
			// 공격 범위 이내로 들어왔다면 공격.
			CreatureState = Define.ECreatureState.Skill;
			return;
		}
		else
		{
			// 공격 범위 밖이라면 추적.
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);

			// 너무 멀어지면 포기.
			float searchDistanceSqr = chaseRange * chaseRange;
			if (distToTargetSqr > searchDistanceSqr)
			{
				_target = null;
				HeroMoveState = Define.EHeroMoveState.None;
				CreatureState = Define.ECreatureState.Move;
			}
			return;
		}
	}
	#endregion

	private void TryResizeCollider()
	{
		// 일단 충돌체 아주 작게.
		ChangeColliderSize(Define.EColliderSize.Small);

		foreach (var hero in Managers.Object.Heroes)
		{
			if (hero.HeroMoveState == Define.EHeroMoveState.ReturnToCamp)
				return;
		}

		// ReturnToCamp가 한 명도 없으면 콜라이더 조정.
		foreach (var hero in Managers.Object.Heroes)
		{
			// 단 채집이나 전투중이면 스킵.
			if (hero.CreatureState == Define.ECreatureState.Idle)
				hero.ChangeColliderSize(Define.EColliderSize.Big);
		}
	}
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

		// TODO
		CreatureState = Define.ECreatureState.Move;

		// Skill
		if (_target.IsValid() == false)
			return;
		
		_target.OnDamaged(this);
	}
}
