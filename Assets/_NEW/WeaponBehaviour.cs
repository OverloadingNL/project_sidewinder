using UnityEngine;

public class WeaponBehaviour : Interactable
{
    [SerializeField] Weapon weapon;
    [SerializeField] AudioClip pickUpSFX;

    AudioSource audioSource;
    PlayerControl playerObject;

    public override void Interact()
    {
        InstantiateInHand();
    }

    private void Start()
    {
        playerObject = FindObjectOfType<PlayerControl>();
        audioSource = GetComponent<AudioSource>();
    }

    private void InstantiateInHand()
    {
        WeaponSystem weaponSystem = playerObject.GetComponent<WeaponSystem>();
        weaponSystem.PutWeaponInHand(weapon);
        audioSource.PlayOneShot(pickUpSFX, 1f);
        Destroy(gameObject);
    }
}
