using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCBase : EffectBase
{
	protected Define.ECreatureState lastState;

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		EffectType = Define.EEffectType.CrowdControl;
		return true;
	}

	public override void ApplyEffect()
	{
		base.ApplyEffect();

		lastState = Owner.CreatureState;
		if (lastState == Define.ECreatureState.OnDamaged)
			return;

		Owner.CreatureState = Define.ECreatureState.OnDamaged;
	}

	public override bool ClearEffect(Define.EEffectClearType clearType)
	{
		if (base.ClearEffect(clearType) == true)
			Owner.CreatureState = lastState;

		return true;
	}

}