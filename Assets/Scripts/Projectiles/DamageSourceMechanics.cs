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
}

public interface IDamageSource
{
    DamageSourceType Type();
}
