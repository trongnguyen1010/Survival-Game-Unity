using System.Collections.Generic;
using UnityEngine;

public class AreaWeaponPrefab : MonoBehaviour
{
    public AreaWeapon weapon;
    public Vector3 targetSize;
    private float timer;
    public List<Enemy> enemiesInRange;
    private float counter;
    void Start()
    {
        weapon = GameObject.Find("Area Weapon").GetComponent<AreaWeapon>();
        //Destroy(gameObject, weapon.duration);
        targetSize = Vector3.one * weapon.stats[weapon.weaponLevel].range;
        transform.localScale = Vector3.zero;
        timer = weapon.stats[weapon.weaponLevel].duration;
        AudioController.Instance.PlaySound(AudioController.Instance.areaWeaponSpawn);
        
    }
    // Update is called once per frame
    void Update()
    {
        // grow and shrink towards targetsize
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, Time.deltaTime * 5);
        // Shrink and only then destroy
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            targetSize = Vector3.zero;
            if (transform.localScale.x == 0f)
            {
                AudioController.Instance.PlaySound(AudioController.Instance.areaWeaponDeSpawn);
                Destroy(gameObject);
            }
        }

        //preiodic damage
        counter -= Time.deltaTime;
        if(counter <= 0)
        {
            counter = weapon.stats[weapon.weaponLevel].speed;
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                enemiesInRange[i].TakeDamage(weapon.stats[weapon.weaponLevel].damage);
            }
        }
    }

    // private void OnTriggerStay2D(Collider2D collider)
    // {
    //     if (collider.CompareTag("Enemy"))
    //     {
    //         Enemy enemy = collider.GetComponent<Enemy>();
    //         enemy.TakeDamage(weapon.damage);
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            enemiesInRange.Add(collider.GetComponent<Enemy>());
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(collider.GetComponent<Enemy>());
        }
    }
}
