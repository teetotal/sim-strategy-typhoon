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
                SetMessage(string.Format("Not enough resources {0}, {1}, {2}", q.type, q.mapId, q.id), null);
                return false;
            }
        }

        return true;
    }
    public void OnCreationFinish(ActionType type, Object obj)
    {
    }
    public void OnDie(ActionType type, Object obj, Object from)
    {    
        //전리품
        if(type == ActionType.ACTOR_DIE)
        {  
            List<Meta.IdQuantity> booties = MetaManager.Instance.GetActorBooty(obj.id, obj.level);
            
            //string sz = "";
            for(int n = 0; n < booties.Count; n++)
            {
                Meta.IdQuantity booty = booties[n];
                //sz += string.Format("\n{0} x{1}", ItemManager.Instance.items[booty.id].name, booty.quantity);
                InventoryManager.Instance.Add(from.tribeId, booty.id, booty.quantity);
            }
            
            SetMessage(string.Format("OnDie from {0} > {1}", from.tribeId, obj.tribeId), booties);
        }
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
                if(meta.level[actor.level].ability.attackDistance < MapManager.Instance.GetDistance(actor.GetCurrentMapId(), targetMapId))
                {
                    SetMessage("too far target to attack", null);
                    return;
                }
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
                if(meta.level[actor.level].ability.attackDistance < MapManager.Instance.GetDistance(actor.GetCurrentMapId(), targetMapId))
                {
                    SetMessage("too far target to attack", null);
                    return;
                }
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
            SetMessage("Finding the load failure", null);
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
    public float elapseMessage; 
    GameObject messageArea;
    Text message;
    GameObject bootiesPanel;
    public void Init()
    {
        elapseMessage = 0;
        messageArea = GameObject.Find("MessageArea");
        message = GameObject.Find("message").GetComponent<Text>();
        bootiesPanel = GameObject.Find("booties");
        messageArea.SetActive(false);
    }
    public void UpdateResourceUI()
    {
        int[] v = new int[3]
        {
            (int)GameStatusManager.Instance.GetResource(0, 0),
            (int)GameStatusManager.Instance.GetResource(0, 1),
            (int)GameStatusManager.Instance.GetResource(0, 2)
        };

        float[] r = new float[2]
        {
            0.7f,
            0.5f
        };
        GameObject.Find("resource").GetComponent<ResourceUI>().SetValues(v, r);
    }
    private void SetMessage(string sz, List<Meta.IdQuantity> booties)
    {
        messageArea.SetActive(true);
        elapseMessage = 0;
        message.text = sz;

        if(booties == null)
            return;
       
        Vector3 panelPosition = bootiesPanel.transform.position;
        Vector2 panelSize = Vector2.zero;
        
        for(int n=0; n < booties.Count; n++)
        {
            GameObject booty = Resources.Load<GameObject>("booty_default");
            Vector2 bootySize = booty.GetComponent<RectTransform>().sizeDelta;
            
            if(n == 0)
            {
                panelSize = new Vector2(
                    bootySize.x * booties.Count,
                    bootySize.y
                );
                bootiesPanel.GetComponent<RectTransform>().sizeDelta = panelSize;
            }

            booty = GameObject.Instantiate(booty
                                        , new Vector3(panelPosition.x + bootySize.x * n - (panelSize.x * 0.5f) + (bootySize.x * 0.5f), panelPosition.y, panelPosition.z)
                                        , Quaternion.identity);

            booty.GetComponentInChildren<RawImage>().texture = Resources.Load<Sprite>(ItemManager.Instance.items[booties[n].id].prefab).texture;
            booty.GetComponentInChildren<Text>().text = string.Format("x{0}", booties[n].quantity);
            booty.transform.SetParent(bootiesPanel.transform);
        }
    }
    public void CheckMessageAvailable(float deltaTime)
    {
        if(!messageArea.activeSelf)
            return;

        elapseMessage += deltaTime;
        if(elapseMessage > 3.0f)
        {
            for(int i = 0; i <bootiesPanel.transform.childCount; i++)
            {
                GameObject.Destroy(bootiesPanel.transform.GetChild(i).gameObject);
            }
            messageArea.SetActive(false);
        }
    }
}