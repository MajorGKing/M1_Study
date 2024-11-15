using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroManager
{
    public Dictionary<int, HeroInfo> AllHeroInfos { get; set; } = new Dictionary<int, HeroInfo>();

    public List<HeroInfo> PickedHeroes
    {
        get { return AllHeroInfos.Values.Where(h => h.OwningState == Define.EHeroOwningState.Picked).ToList(); }
    }

    public List<HeroInfo> OwnedHeroes
    {
        get { return AllHeroInfos.Values.Where(h => h.OwningState == Define.EHeroOwningState.Owned).ToList(); }
    }

    public List<HeroInfo> UnownedHeroes
    {
        get { return AllHeroInfos.Values.Where(h => h.OwningState == Define.EHeroOwningState.Unowned).ToList(); }
    }

    public HeroInfo GetHeroInfo(int templateId)
    {
        AllHeroInfos.TryGetValue(templateId, out HeroInfo heroInfo);
        return heroInfo;
    }

    public HeroSaveData MakeHeroInfo(int templateId)
    {
        if (Managers.Data.HeroInfoDic.TryGetValue(templateId, out Data.HeroInfoData heroInfoData) == false)
            return null;

        HeroSaveData saveData = new HeroSaveData()
        {
            DataId = heroInfoData.DataId,
            Level = 1,
            Exp = 0,
            OwningState = Define.EHeroOwningState.Unowned
        };

        AddHeroInfo(saveData);
        return saveData;
    }

    public HeroInfo AddHeroInfo(HeroSaveData saveData)
    {
        HeroInfo heroInfo = HeroInfo.MakeHeroInfo(saveData);
        if (heroInfo == null)
            return null;

        AllHeroInfos.Add(heroInfo.TemplateId, heroInfo);
        return heroInfo;
    }

    public Hero PickHero(int templateId, Vector3Int joinCellPos)
	{
		HeroInfo heroInfo = GetHeroInfo(templateId);
		if (heroInfo == null)
		{
			Debug.Log("영웅존재안함");
			return null;
		}

		heroInfo.OwningState = Define.EHeroOwningState.Picked;

		Hero hero = null;

		if (joinCellPos == Vector3.zero)
		{
			Vector3Int randCellPos = Managers.Game.GetNearbyPosition(null, Managers.Object.Camp.CellPos);
			hero = Managers.Object.Spawn<Hero>(randCellPos, templateId);
			hero.SetCellPos(randCellPos, true);
		}
		else
		{
			hero = Managers.Object.Spawn<Hero>(joinCellPos, templateId);
			hero.SetCellPos(joinCellPos, true);
		}

		Managers.Game.BroadcastEvent(Define.EBroadcastEventType.ChangeCrew, 0);

		return hero;
	}

    public void UnpickHero(int heroId)
	{
		if (AllHeroInfos.TryGetValue(heroId, out HeroInfo info) == false)
			return;

		if (info.OwningState == Define.EHeroOwningState.Picked)
		{
			info.OwningState = Define.EHeroOwningState.Owned;

			var heroes = Managers.Object.Heroes.ToList();
			Hero despawnHero = heroes.Find(hero => hero.DataTemplateID == heroId);
			Managers.Object.Despawn(despawnHero);

			Managers.Game.BroadcastEvent(Define.EBroadcastEventType.ChangeCrew, heroId);
		}
	}

    public void AcquireHeroCard(int heroId, int exp)
	{
		if (AllHeroInfos.TryGetValue(heroId, out HeroInfo heroInfo) == false)
			return;

		if (heroInfo.OwningState == Define.EHeroOwningState.Unowned)
		{
			heroInfo.OwningState = Define.EHeroOwningState.Owned;
			heroInfo.Exp += exp;
		}
		else
		{
			heroInfo.Exp += exp;
		}
	}

    public void AddUnknownHeroes()
	{
		foreach (Data.HeroInfoData heroInfo in Managers.Data.HeroInfoDic.Values.ToList())
		{
			if (AllHeroInfos.ContainsKey(heroInfo.DataId))
				continue;

			MakeHeroInfo(heroInfo.DataId);
		}
	}

}
