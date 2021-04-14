public enum BuildingType
{
    FACTORY = 0,
    DECORATION,
    MARKET,
    MAX
}
public enum ActionType : int
{
    BUILDING_CREATE = 0,
    BUILDING_DESTROY,
    BUILDING_UPGRADE,
    BUILDING_MAX,

    ACTOR_CREATE,
    ACTOR_MOVING,
    ACTOR_FLYING,
    ACTOR_ATTACK,
    ACTOR_MAX,

    MOB_CREATE,
    MOB_MOVING,
    MOB_FLYING,
    MOB_ATTACK,
    MOB_MAX,
    MAX,
}
