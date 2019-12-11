using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class SelfHealBehaviour : AbilityBehaviour
{
    PlayerControl player = null;

    void Start()
    {
        player = GetComponent<PlayerControl>();
    }

    public override void Use(GameObject target)
    {
        PlayAbilitySound();
        var playerHealth = player.GetComponent<HealthSystem>();
        playerHealth.Heal((config as SelfHealConfig).GetExtraHealth());
        PlayParticleEffect();
        PlayAbilityAnimation();
    }
}