using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

public class Creature : BaseObject
{
    public Data.CreatureData CreatureData { get; private set; }
    public Define.ECreatureType CreatureType { get; protected set; } = Define.ECreatureType.None;
    

    #region Stats
	public float Hp { get; set; }
	public float MaxHp { get; set; }
	public float MaxHpBonusRate { get; set; }
	public float HealBonusRate { get; set; }
	public float HpRegen { get; set; }
	public float Atk { get; set; }
	public float AttackRate { get; set; }
	public float Def { get; set; }
	public float DefRate { get; set; }
	public float CriRate { get; set; }
	public float CriDamage { get; set; }
	public float DamageReduction { get; set; }
	public float MoveSpeedRate { get; set; }
	public float MoveSpeed { get; set; }
	#endregion

    protected Define.ECreatureState _creatureState = Define.ECreatureState.None;
    public virtual Define.ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                _creatureState = value;
                UpdateAnimation();
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Creature;
        //CreatureState = Define.ECreatureState.Idle;
        return true;
    }

    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        CreatureData = Managers.Data.CreatureDic[templateID];
        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        // Collider
		Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffstY);
		Collider.radius = CreatureData.ColliderRadius;

        // RigidBody
		RigidBody.mass = CreatureData.Mass;

        // Spine
		SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(CreatureData.SkeletonDataID);
		SkeletonAnim.Initialize(true);

        // Register AnimEvent
		if (SkeletonAnim.AnimationState != null)
		{
			SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
			SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
		}

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함.
		// 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을같이 계산함.
		SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
		sg.sortingOrder = SortingLayers.CREATURE;

        // Skills
		// CreatureData.SkillIdList;

		// Stat
		MaxHp = CreatureData.MaxHp;
		Hp = CreatureData.MaxHp;
		Atk = CreatureData.Atk;
		MoveSpeed = CreatureData.MoveSpeed;

		// State
		CreatureState = Define.ECreatureState.Idle;

    }

    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case Define.ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            case Define.ECreatureState.Skill:
                PlayAnimation(0, AnimName.ATTACK_A, true);
                break;
            case Define.ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;
            case Define.ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;
            default:
                break;
        }
    }

    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;

    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (CreatureState)
            {
                case Define.ECreatureState.Idle:
                    UpdateIdle();
                    break;
                case Define.ECreatureState.Move:
                    UpdateMove();
                    break;
                case Define.ECreatureState.Skill:
                    UpdateSkill();
                    break;
                case Define.ECreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateSkill() { }
    protected virtual void UpdateDead() { }
    #endregion

    	#region Wait
	protected Coroutine _coWait;

	protected void StartWait(float seconds)
	{
		CancelWait();
		_coWait = StartCoroutine(CoWait(seconds));
	}

	IEnumerator CoWait(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		_coWait = null;
	}

	protected void CancelWait()
	{
		if (_coWait != null)
			StopCoroutine(_coWait);
		_coWait = null;
	}
	#endregion
}
