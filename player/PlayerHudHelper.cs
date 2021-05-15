using System.Text;
using Godot;

namespace GodotFPS.player_helpers
{
    public class PlayerHudHelper
    {
        private Label _uiStatusLabel;
        private Player _player;

        public void OnReady(Player player)
        {
            _player = player;
            _uiStatusLabel = player.GetNode<Label>("HUD/Panel/Gun_label");
        }

        public void ProcessHud(float delta, PlayerWeaponHelper weaponHelper)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"HEALTH: {_player.Health}");
            if (weaponHelper.CurrentWeaponName != "UNARMED"
                && weaponHelper.CurrentWeaponName != "KNIFE")
            {
                var currentWeapon = weaponHelper.CurrentWeapon;
                sb.Append($"AMMO: {currentWeapon.AmmoInWeapon}");
                sb.Append("/");
                sb.AppendLine(currentWeapon.SpareAmmo.ToString());
            }

            sb.Append($"{weaponHelper.CurrentGrenadeName}: ");
            sb.AppendLine(weaponHelper.CurrentGrenadeAmount.ToString());
            _uiStatusLabel.Text = sb.ToString();
        }
    }
}