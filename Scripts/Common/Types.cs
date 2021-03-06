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
    BUILDING_DEFENSE,
    BUILDING_UNDER_ATTACK,
    BUILDING_MAX,

    ACTOR_CREATE,
    ACTOR_MOVING,
    ACTOR_FLYING,
    ACTOR_MOVING_1_STEP,
    ACTOR_ATTACK,
    ACTOR_UNDER_ATTACK,
    ACTOR_DIE,
    ACTOR_DIE_FROM_DESTROYED_BUILDING,
    ACTOR_LOAD_RESOURCE,
    ACTOR_DELIVERY,
    ACTOR_MAX,

    MOB_CREATE,
    MOB_MOVING,
    MOB_FLYING,
    MOB_ATTACK,
    MOB_UNDER_ATTACK,
    MOB_DIE,
    MOB_MAX,

    NEUTRAL_CREATE,
    NEUTRAL_DESTROY,
    NEUTRAL_UPGRADE,
    NEUTRAL_MAX,
    MAX,
}

public enum ItemType
{
    RESOURCE = 0, //자원
    GACHA_MATERIAL, //강화 재료
    MAX
}

//장착용 아이템
public enum ItemInstallationType
{
    HEAD = 0,
    L_SHOULDER,
    R_SHOULDER,
    MAX
}

//발동형 아이템
public enum ItemForceType
{
    MOVING,
    ATTACK,
    ATTACK_DISTANCE,
    ATTACK_SPEED,
    MAX
}
