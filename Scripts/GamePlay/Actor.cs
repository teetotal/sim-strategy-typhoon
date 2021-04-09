using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Actor : Object
{
    public BuildingObject attachedBuilding; //소속된 건물 정보
    //전투의 경우, 수량 정보도 필요할 수 있음.
    public int currentHeadcount; //현재 인원. 전체 인원은 type과 level로 파악

    public override bool Create(int mapId, int id)
    {
        //생성 위치 찾기.
        mapId = MapManager.Instance.AssignNearEmptyMapId(mapId);
        if(mapId == -1)
            return false;
        
        this.mapId = mapId;
        this.id = id;
        
        Action action = new Action();
        action.type = ActionType.ACTOR_CREATE;
        action.currentTime = 0;
        action.totalTime = MetaManager.Instance.actorInfo[id].createTime;
        actions.Add(action);

        //prefab 생성
        Vector3 position = MapManager.Instance.GetVector3FromMapId(mapId);
        GameObject obj = Resources.Load<GameObject>(MetaManager.Instance.actorInfo[id].prefab);
        obj = GameObject.Instantiate(obj, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity);
        obj.tag = MetaManager.Instance.GetTag(MetaManager.TAG.ACTOR);
        obj.name = mapId.ToString();
        GameObject parent = MapManager.Instance.defaultGameObjects[mapId];
        obj.transform.SetParent(parent.transform);

        gameObject = obj;

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, pos, Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);

        return true;
    }
    public void SetMoving(int targetMapId)
    {
        Action action = new Action();
        action.type = ActionType.ACTOR_MOVING;
        action.currentTime = 0;

        //Astar
        List<int> route = new List<int>();
        Astar astar = new Astar(MapManager.Instance.map);
        Vector2Int from = MapManager.Instance.GetMapPosition(GetCurrentPositionMapId());
        Vector2Int to = MapManager.Instance.GetMapPosition(targetMapId);
        Stack<Astar.Pos> stack = astar.Search(new Astar.Pos(from.x, from.y), new Astar.Pos(to.x, to.y));
        if(stack == null)
            return;
        
        while(stack.Count > 0)
        {
            int id = MapManager.Instance.GetMapId(new Vector2Int(stack.Peek().x, stack.Peek().y));
            route.Add(id);
            stack.Pop();
        }
        action.totalTime = route.Count;
        action.values = route;

        //이전 이동 액션을 제거
        RemoveActionType(ActionType.ACTOR_MOVING);
        //새로운 액션을 추가
        actions.Add(action);
    }
    int GetCurrentPositionMapId()
    {
        for(int n = 0; n < actions.Count; n++)
        {
            if(actions[n].type == ActionType.ACTOR_MOVING)
            {
                int idx = (int)actions[n].currentTime;
                return actions[n].values[idx];
            }
        }
        return this.mapId;
    }
    public override void Update()
    {
        if(progress != null) //progress
        {
            progress.transform.position = GetProgressPosition(); //position
        }
        List<int> removeActionIds = new List<int>();
        for(int n = 0; n < actions.Count; n++)
        {
            Action action = actions[n];
            
            action.currentTime += Time.deltaTime;
            actions[n] = action;

            switch(action.type)
            {
                case ActionType.ACTOR_CREATE:
                    progress.GetComponent<Slider>().value = action.currentTime / action.totalTime;
                    //Debug.Log(string.Format("{0}-{1}/{2}", mapId, action.currentTime, action.totalTime));
                    break;
                case ActionType.ACTOR_MOVING:
                    Moving(action);
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                removeActionIds.Add(n);
                switch(action.type)
                {
                    case ActionType.ACTOR_CREATE:
                        GameObject.Destroy(progress);
                        progress = null;
                        break;
                }
            }
        }

        RemoveActions(removeActionIds);
    }
    bool Moving(Action action)
    {
        List<int> route = action.values;

        GameObject actor = this.gameObject;
        int idx = (int)action.currentTime;
        if(idx > route.Count - 1)
        {
            return false;
        }
        float ratio = action.currentTime % 1.0f;
        Vector3 posNext;
        Vector3 pos = MapManager.Instance.GetVector3FromMapId(route[idx]);//GetRoutePosition(idx);
        if(idx <= route.Count - 2)
        {
            posNext = MapManager.Instance.GetVector3FromMapId(route[idx + 1]);//GetRoutePosition(idx + 1);
             //Vector3 diff = (posNext - pos) * ratio * 1.0f;
            actor.transform.position = Vector3.Lerp(pos, posNext, ratio) + new Vector3(0, 0.1f, 0);//pos + diff + new Vector3(0, 0.1f, 0);

            Vector3 target = posNext + new Vector3(0, 0.1f, 0);
            
            Vector3 dir = target - actor.transform.position;
            actor.transform.rotation = Quaternion.Lerp(actor.transform.rotation, Quaternion.LookRotation(dir), ratio);
        } 
        else
        {
            return false;
        }
        return true;
    }
}