using Godot;

namespace GodotFPS
{
    public abstract class WeaponBase : Spatial
    {
        public abstract int Damage { get; }
        public abstract int AmmoInMag { get; }
        public abstract bool CanReload { get; }
        public abstract bool CanRefill { get; }
        public abstract string ReloadingAnimationName { get; }
        public abstract string IdleAnimationName { get; }
        public abstract string FireAnimationName { get; }
        public abstract bool Enabled { get; set; }
        public abstract int AmmoInWeapon { get; set; }
        public abstract int SpareAmmo { get; set; }
        
        public abstract Player Player { get; set; }

        public abstract void Fire();
        public abstract bool Equip();
        public abstract bool Unequip();

        public virtual bool Reload()
        {
            
            var canReload = Player.AnimationManager.CurrentState == IdleAnimationName;

            if (SpareAmmo <= 0 || AmmoInWeapon == AmmoInMag) canReload = false;

            if (!canReload) return false;

            var ammoNeeded = AmmoInMag - AmmoInWeapon;
            if (SpareAmmo >= ammoNeeded)
            {
                SpareAmmo -= ammoNeeded;
                AmmoInWeapon += AmmoInMag;
            }
            else
            {
                AmmoInWeapon += SpareAmmo;
                SpareAmmo = 0;
            }

            Player.AnimationManager.SetAnimation(ReloadingAnimationName);
            Player.CreateSound("Gun_cock", Player.GlobalTransform.origin);
            return true;
        }
        
        public abstract void Reset();
    }
}