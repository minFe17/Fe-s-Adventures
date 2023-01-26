using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range, Poison };
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type != Type.Melee && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
            GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
            bulletRigid.velocity = bulletPos.forward * 50;

            yield return null;

            GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
            Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
            Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
            caseRigid.AddForce(caseVec, ForceMode.Impulse);
            caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }

}
