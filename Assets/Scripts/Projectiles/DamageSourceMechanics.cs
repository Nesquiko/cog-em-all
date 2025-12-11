public enum DamageSourceType
{
    Bullet,
    Beam,
    Shell,
    Flame,
    Wall,
    Mine,
    Effect,
    Airstrike,
    Bomber,
    IceShatter,
}

public interface IDamageSource
{
    DamageSourceType Type();
}
