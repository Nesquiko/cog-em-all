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
}

public interface IDamageSource
{
    DamageSourceType Type();
}
