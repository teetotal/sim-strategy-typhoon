using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    NODE_CREATE,
    NODE_DESTROY,
    NODE_UPGRADE,

    BUILDING_CREATE,
    BUILDING_DESTROY,
    BUILDING_UPGRADE,

    ACTOR_CREATE,
    ACTOR_MOVING,
    ACTOR_ATTACK,
    MAX,
}

public enum ActorType
{
    OWNER,
    WARRIOR,
    CARRIER,
    WORKER,
    MAX
}