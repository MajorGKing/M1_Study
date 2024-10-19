using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
	public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
	public HashSet<Env> Envs { get; } = new HashSet<Env>();

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
	public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
	#endregion

    public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
		go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();

		if (obj.ObjectType == Define.EObjectType.Creature)
		{

			// Data Check
			if (templateID != 0 && Managers.Data.CreatureDic.TryGetValue(templateID, out Data.CreatureData data) == false)
			{
				Debug.LogError($"ObjectManager Spawn Creature Failed! TryGetValue TemplateID : {templateID}");
				return null;
			}

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

			creature.SetInfo(templateID);

		}
		else if (obj.ObjectType == Define.EObjectType.Projectile)
		{
			// TODO
		}
		else if (obj.ObjectType == Define.EObjectType.Env)
		{
			// Data Check
			if (templateID != 0 && Managers.Data.EnvDic.TryGetValue(templateID, out Data.EnvData data) == false)
			{
				Debug.LogError($"ObjectManager Spawn Env Failed! TryGetValue TemplateID : {templateID}");
				return null;
			}

			obj.transform.parent = EnvRoot;

			Env env = go.GetComponent<Env>();
			Envs.Add(env);

			env.SetInfo(templateID);
		}

		return obj as T;
    } 
}
