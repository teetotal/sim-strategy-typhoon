public enum TAG
{
    BOTTOM = 0,
    ENVIRONMENT,
    BUILDING,
    ACTOR,
    MOB,
    NEUTRAL,
    MAX
}
public enum BuildingType
{
    FACTORY = 0,
    DECORATION,
    MARKET,
    MAX
}
public enum ActionType
{
    BUILDING_CREATE = 0,
    BUILDING_DESTROY,
    BUILDING_UPGRADE,
    BUILDING_MAX,

    ACTOR_CREATE,
    ACTOR_MOVING,
    ACTOR_FLYING,
    ACTOR_MOVING_1_STEP,
    ACTOR_ATTACK,
    ACTOR_UNDER_ATTACK,
    ACTOR_DIE,
    ACTOR_MAX,

    MOB_CREATE,
    MOB_MOVING,
    MOB_FLYING,
    MOB_ATTACK,
    MOB_MAX,

    NEUTRAL_CREATE,
    NEUTRAL_DESTROY,
    NEUTRAL_UPGRADE,
    NEUTRAL_MAX,
    MAX,
}


