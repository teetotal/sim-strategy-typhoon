using System.Collections.Generic;
using UnityEngine;

public struct UnderAttack
{
    public Object from;
    public int amount;

    public UnderAttack(Object from, int amount)
    {
        this.from = from;
        this.amount = amount;
    }
}

//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public float totalTime;       //Action이 적용되는 총 시간
    public float currentTime;     //현재까지 진행된 시간
    public List<int> values;      //기타 추가 정보. 이동시 A* route같은거 담는 용도

    public Action(ActionType type, float totalTime = 0, List<int> values = null)
    {
        this.type = type;
        this.currentTime = 0;
        this.totalTime = totalTime;
        this.values = values;
    }

    public float GetProgression()
    {
        if(this.totalTime == 0)
            return 0;
        if(this.currentTime > this.totalTime)
            return this.values[this.values.Count - 1];

        return (this.currentTime / this.totalTime) * this.values.Count;
    }
}
/* --------------------------- */
//중립 지역 건물 정보
public class NeutralBuilding: Object
{
    public override bool AddAction(QNode node)
    {
        return true;
    }
    public override bool Create(int tribeId, int mapId, int id)
    {
        this.tribeId = tribeId;
        this.mapId = mapId;
        this.id = id;

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, pos, Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);
        progress.SetActive(false);

        return true;
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

/* --------------------------- */
public struct QNode
{
    public ActionType type;
    public int tribeId;
    public int mapId; //mapid
    public int id;
    public bool immediately;
    public List<int> values;    //caller쪽과 protocol을 맞춰야 한다.
    public int insertIndex;

    public QNode(ActionType type, int tribeId, int mapId, int id, List<int> values, bool immediately, int insertIndex)
    {
        this.tribeId = tribeId;
        this.type = type;
        this.mapId = mapId;
        this.id = id;
        this.values = values;
        this.immediately = immediately;
        this.insertIndex = insertIndex;
    }
}
/* --------------------------- */
//시대 정보
public struct TimeNode
{
    public string name; //시대 이름
    public int maxTime; //종료 시점
}