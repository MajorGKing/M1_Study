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

		Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
		Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;
		Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
		Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

		return true;
	}

	void Update()
	{
		transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
	}

	private void HandleOnMoveDirChanged(Vector2 dir)
	{
		_moveDir = dir;
		Debug.Log(dir);
	}

	private void HandleOnJoystickStateChanged(Define.EJoystickState joystickState)
	{
		switch (joystickState)
		{
			case Define.EJoystickState.PointerDown:
				CreatureState = Define.ECreatureState.Move;
				break;
			case Define.EJoystickState.Drag:
				break;
			case Define.EJoystickState.PointerUp:
				CreatureState = Define.ECreatureState.Idle;
				break;
			default:
				break;
		}
	}
}
