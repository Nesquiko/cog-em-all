public enum DamageSourceType
{
    Bullet,
    Beam,
    Shell,
    Flame,
    Wall,
    Mine,
    Effect,
}

public interface IDamageSource
{
    DamageSourceType Type();
}
