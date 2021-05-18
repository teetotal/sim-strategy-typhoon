using System.Collections.Generic;
using UnityEngine;
/* --------------------------- */
//중립 지역 건물 정보
public class NeutralBuilding: Object
{
    public override bool AddAction(QNode node)
    {
        return true;
    }
    public override bool Create(int tribeId, int mapId, int id, bool isInstantiate, float rotation)
    {
        this.Init(-1, id, mapId, TAG.NEUTRAL, -1, 0, rotation);

        if(isInstantiate)
        {
            Instantiate();
        }

        return true;
    }
    public override void Instantiate()
    {
        Meta.Neutral meta = MetaManager.Instance.neutralInfo[id];
        Instantiate(meta.prefab, false);
    }
    public void Destroy()
    {
        this.Release();
    }
    public override void Update()
    {
    }
    public override void UpdateUnderAttack()
    {
    }
    public override void UpdateDefence()
    {

    }
    public override void UpdateEarning()
    {
    }
}
