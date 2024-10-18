using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
	public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();

    #region Roots
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
		if (root == null)
			root = new GameObject { name = name };

		return root.transform;
    }

    public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
	public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
	#endregion

    public T Spawn<T>(Vector3 position) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
		go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();

        if (obj.ObjectType == Define.EObjectType.Creature)
		{
			Creature creature = go.GetComponent<Creature>();
			switch (creature.CreatureType)
			{
				case Define.ECreatureType.Hero:
					obj.transform.parent = HeroRoot;
					Hero hero = creature as Hero;
					Heroes.Add(hero);
					break;
				case Define.ECreatureType.Monster:
					obj.transform.parent = MonsterRoot;
					Monster monster = creature as Monster;
					Monsters.Add(monster);
					break;
			}
		}
		else if (obj.ObjectType == Define.EObjectType.Projectile)
		{
			// TODO
		}
		else if (obj.ObjectType == Define.EObjectType.Env)
		{
			// TODO
		}

		return obj as T;
    } 
}
