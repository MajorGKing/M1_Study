using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifier
{
	public readonly float Value;
	public readonly Define.EStatModType Type;
	public readonly int Order;
	public readonly object Source;

	public StatModifier(float value, Define.EStatModType type, int order, object source)
	{
		Value = value;
		Type = type;
		Order = order;
		Source = source;
	}

	public StatModifier(float value, Define.EStatModType type) : this(value, type, (int)type, null) { }

	public StatModifier(float value, Define.EStatModType type, int order) : this(value, type, order, null) { }

	public StatModifier(float value, Define.EStatModType type, object source) : this(value, type, (int)type, source) { }
}