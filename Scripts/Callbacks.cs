using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
    Context에 등록되는 On~ 함수들
*/
public class Callbacks
{
    //생성 이벤트
    public bool OnCreationEvent(QNode q)
    {
        if(q.immediately)
            return true;
        //resource 차감
        List<Meta.ResourceIdAmount> list = null;
        switch(q.type)
        {
            case ActionType.ACTOR_CREATE:
                list = MetaManager.Instance.actorInfo[q.id].level[0].wage;
                break;
            case ActionType.BUILDING_CREATE:
                list = MetaManager.Instance.buildingInfo[q.id].level[0].costs;
                break;
        }
        if(list != null)
        {
            if(GameStatusManager.Instance.Spend(q.tribeId, list))
                UpdateResourceUI();
            else
            {
                Debug.Log(string.Format("Not enough resources {0}, {1}, {2}", q.type, q.mapId, q.id));
                return false;
            }
        }

        return true;
    }
    public void OnCreationFinish(ActionType type, Object obj)
    {
        Debug.Log(string.Format("OnCreationFinish {0}, {1}, {2}, {3}", type, obj.gameObject.tag, obj.mapId, obj.id));
    }
    public void SetDelivery(Actor actor, int targetMapId, TAG targetBuildingTag)
    {
        Meta.Actor meta = MetaManager.Instance.actorInfo[actor.id];
        ActionType type = meta.flying ? ActionType.ACTOR_FLYING: ActionType.ACTOR_MOVING;
        //routine 추가
        actor.SetRoutine(new List<QNode>()
        {
            new QNode(type, actor.tribeId, actor.mapId, actor.attachedBuilding.mapId, null, false, -1), //home으로 가서
            new QNode(ActionType.ACTOR_LOAD_RESOURCE, actor.tribeId, actor.mapId, actor.attachedBuilding.mapId, null, false, -1), //적재
            new QNode(type, actor.tribeId, actor.mapId, targetMapId, null, false, -1), // 시장으로 이동
            new QNode(ActionType.ACTOR_DELIVERY, actor.tribeId, actor.mapId, targetMapId, new List<int>() { (int)targetBuildingTag }, false, -1) //판매
        });
    }

    // 모든 선택 이벤트 통합.
    public void OnSelected(TAG tag, int mapId, int id, GameObject gameObject)
    {
        if(tag == TAG.BOTTOM || tag == TAG.ENVIRONMENT)
            return;

        //Debug.Log(string.Format("OnSelected {0} {1} {2}", tag, mapId, id));
        Object obj = Util.GetObject(mapId, tag);
        SelectionUI.Instance.Activate(tag, gameObject, new string[1] { 
            string.Format("{0} {1} {2}", Util.GetNameInGame(tag, id),  obj.tribeId, mapId.ToString() )
            });
    }
    //Actor 모든 행동 이벤트
    public void OnActorAction(Actor actor, TAG tag, int targetMapId)
    {
        //Debug.Log(string.Format("OnAction {0}, {1}, {2}, {3}", mapId, id, tag, targetMapId));
        Meta.Actor meta = MetaManager.Instance.actorInfo[actor.id];
        Object targetObject = Util.GetObject(targetMapId, tag);
        switch(tag)
        {
            case TAG.BUILDING:
                if(actor.tribeId != targetObject.tribeId && actor.SetFollowObject(targetMapId, TAG.BUILDING))
                {
                   Updater.Instance.AddQ(ActionType.ACTOR_ATTACK, actor.tribeId, actor.mapId, -1, null, false);
                }
                break;
            case TAG.NEUTRAL:
                NeutralBuilding targetBuilding = NeutralManager.Instance.objects[targetMapId];
                Meta.Neutral targetMeta = MetaManager.Instance.neutralInfo[targetBuilding.id]; 
                if(targetMeta.type == (int)BuildingType.MARKET)
                    SetDelivery(actor, targetMapId, TAG.NEUTRAL);
                break;
            case TAG.ACTOR:
                if(actor.tribeId != targetObject.tribeId && actor.SetFollowObject(targetMapId, TAG.ACTOR))
                {
                    Updater.Instance.AddQ(ActionType.ACTOR_ATTACK, actor.tribeId, actor.mapId, -1, null, false);
                }
                break;
            case TAG.MOB:
                actor.followObject = MobManager.Instance.mobs[targetMapId];
                Updater.Instance.AddQ(ActionType.ACTOR_ATTACK, actor.tribeId, actor.mapId, -1, null, false);
                break;
            case TAG.BOTTOM:
                if(actor.mapId != targetMapId && MapManager.Instance.IsEmptyMapId(targetMapId))
                {
                    Updater.Instance.AddQ(
                        meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING, 
                        actor.tribeId,
                        actor.mapId, 
                        targetMapId, 
                        null,
                        false);
                }
                break;
        }
        SelectionUI.Instance.Hide();
    }
    public void OnAttack(Object from, Object to, int amount)
    {
        //Debug.Log(string.Format("OnAttack {0} -> {1} attack: {2}, HP {3}", from.mapId, to.mapId, amount, to.currentHP));
        //if(to.currentHP <= 0) Debug.Log("OnAttack Die");  
    }
    
    public void OnLoadResource(Actor actor, int targetBuildingMapId)
    {
        //짐 싣기
        GameObject selectedObj = actor.gameObject;
        //Debug.Log(string.Format("OnClickForUpgradingActor {0}-{1}", mapId, actorId));
        GameObject o = Resources.Load<GameObject>("load");
        o = GameObject.Instantiate(o);
        o.name = "load";
        o.transform.SetParent(selectedObj.transform);
        Vector3 size = selectedObj.GetComponent<BoxCollider>().size;
        o.transform.localPosition = new Vector3(0, size.y, 0);
    }
    public void OnDelivery(Actor actor, int targetBuildingMapId, TAG targetBuildingTag)
    {
        Transform load = actor.gameObject.transform.Find("load");
        if(load == null)
        {
            Debug.LogError("Finding the load failure");
        }
        else
        {
            GameObject.DestroyImmediate(load.gameObject);
        }
        //resource 차감
        Meta.Building metaBuilding = MetaManager.Instance.buildingInfo[actor.attachedBuilding.id];
        Meta.Actor metaActor = MetaManager.Instance.actorInfo[actor.id];

        for(int n = 0; n < metaBuilding.level[actor.attachedBuilding.level].output.Count; n++)
        {
            int resourceId = metaBuilding.level[actor.attachedBuilding.level].output[n].resourceId;
            int amount = metaActor.level[actor.level].ability.carring;
            //tribe
            GameStatusManager.Instance.resourceInfo[0][resourceId] -= amount;
            GameStatusManager.Instance.resourceInfo[0][MarketManager.Instance.GetStandardResource()] += 
                MarketManager.Instance.Exchange(targetBuildingMapId, resourceId, amount);
        }
        UpdateResourceUI();
    }
    public bool CheckDefenseAttack(Object target, Object from)
    {
        if(target.tribeId != from.tribeId)
            return true;
        return false;
    }

    public void OnEarning(Object obj, bool success)
    {
        if(success)
            UpdateResourceUI();
        else
        {
            //임금을 받지 못했을때 이벤트 처리
        }
    }

    //-------------------------------------------
    /*
    */
    public void UpdateResourceUI()
    {
        for(int n = 0; n < 3; n++)
        {
            GameObject.Find("resource" + (n+1).ToString()).GetComponentInChildren<Text>().text = string.Format("{0} {1}", 
                    MetaManager.Instance.resourceInfo[n], 
                    GameStatusManager.Instance.GetResource(0, n));
        }
    }
}