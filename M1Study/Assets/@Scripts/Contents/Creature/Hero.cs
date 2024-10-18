using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Creature
{
    Vector2 _moveDir = Vector2.zero;

    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = Define.ECreatureType.Hero;
		CreatureState = Define.ECreatureState.Idle;
		Speed = 5.0f;

		return true;
	}
}
