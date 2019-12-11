﻿using UnityEngine;
[CreateAssetMenu(menuName = ("Special Ability/Area Effect"))]
public class AreaEffectConfig : AbilityConfig
{
    [Header("Area Effect Specific")]
    [SerializeField] float radius = 5f;
    [SerializeField] float damageToEachTarget = 15f;

    public override AbilityBehaviour GetBehaviourComponent(GameObject objectToAttachTo)
    {
        return objectToAttachTo.AddComponent<AreaEffectBehaviour>();
    }

    public float GetDamageToEachTarget()
    {
        return damageToEachTarget;
    }

    public float GetRadius()
    {
        return radius;
    }
}