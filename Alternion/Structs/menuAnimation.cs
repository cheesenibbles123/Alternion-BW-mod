using UnityEngine;

namespace Alternion.Structs
{
    public struct menuAnimation
    {
        public AnimationClip animation;
        public bool weaponVisible;
        public string clipName;
        public menuAnimation(AnimationClip clip, bool visibleWeapon, string nameOfClip)
        {
            animation = clip;
            weaponVisible = visibleWeapon;
            clipName = nameOfClip;
        }
    }
}
