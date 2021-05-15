using Godot;

namespace GodotFPS
{
    public class TurretBodies : StaticBody
    {
        [Export] public NodePath PathToTurretRoot { get; set; }

        public void BulletHit(int damage, Transform bulletHitPos)
        {
            if (PathToTurretRoot == null) return;
            GetNode<Turret>(PathToTurretRoot).BulletHit(damage, bulletHitPos);
        }
    }
}