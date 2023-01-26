using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public int score;

    public GameManager manager;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;

    public bool isChase;
    public bool isAttack;
    public bool isDead;
    int PoisonDamage = 3;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void Targerting()
    {
        if (!isDead && enemyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;

            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;

            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        Targerting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (curHealth > 0)
        {
            if (other.tag == "Melee")
            {
                Weapon weapon = other.GetComponent<Weapon>();
                curHealth -= weapon.damage;
                Vector3 reactVec = transform.position - other.transform.position;
                StartCoroutine(OnDamage(reactVec, false));
            }
            else if (other.tag == "Bullet")
            {
                Bullet bullet = other.GetComponent<Bullet>();
                curHealth -= bullet.damage;
                Vector3 reactVec = transform.position - other.transform.position;
                Destroy(other.gameObject);

                StartCoroutine(OnDamage(reactVec, false));
            }
            else if (other.tag == "Poison")
            {
                Bullet bullet = other.GetComponent<Bullet>();
                curHealth -= bullet.damage;
                Vector3 reactVec = transform.position - other.transform.position;
                Destroy(other.gameObject);
                StartCoroutine(OnDamage(reactVec, false));
                StartCoroutine(OnPoisonDamage(reactVec));
            }
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;

            reactVec = reactVec.normalized;
            rigid.AddForce(reactVec * 3, ForceMode.Impulse);
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;
            gameObject.layer = 14;  //EnemyDead 레이어가 14번이라서
            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");
            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);


            switch (enemyType)
            {
                case Type.A:
                    manager.enemyCntA--;
                    if (manager.enemyCntA <= 0)
                    {
                        manager.enemyCntA = 0;
                    }
                    break;

                case Type.B:
                    manager.enemyCntB--;
                    if (manager.enemyCntB <= 0)
                    {
                        manager.enemyCntB = 0;
                    }
                    break;

                case Type.C:
                    manager.enemyCntC--;
                    if (manager.enemyCntC <= 0)
                    {
                        manager.enemyCntC = 0;
                    }
                    break;

                case Type.D:
                    manager.enemyCntD--;
                    if (manager.enemyCntD <= 0)
                    {
                        manager.enemyCntD = 0;
                    }
                    break;
            }

            if (enemyType != Type.D)
            {
                if (isGrenade)
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up;
                    rigid.freezeRotation = false;
                    rigid.AddForce(reactVec * 10, ForceMode.Impulse);
                    rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
                }
                else
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up;
                    rigid.AddForce(reactVec * 5, ForceMode.Impulse);

                }        
            }
            Destroy(gameObject, 4);
        }
    }

    IEnumerator OnPoisonDamage(Vector3 reactVec)
    {
        for (int i = 0; i <= 7; i++)
        {
            yield return new WaitForSeconds(2f);
            curHealth -= PoisonDamage;
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            if (curHealth > 0)
            {
                foreach (MeshRenderer mesh in meshs)
                    mesh.material.color = Color.white;

                reactVec = reactVec.normalized;
                rigid.AddForce(reactVec * 3, ForceMode.Impulse);
            }
            else
            {
                foreach (MeshRenderer mesh in meshs)
                    mesh.material.color = Color.gray;
                gameObject.layer = 14;
                isDead = true;

                isChase = false;
                nav.enabled = false;
                anim.SetTrigger("doDie");
                Player player = target.GetComponent<Player>();
                player.score += score;
                int ranCoin = Random.Range(0, 3);
                Instantiate(coins[ranCoin], transform.position, Quaternion.identity);
                

                switch(enemyType)
                {
                case Type.A:
                    manager.enemyCntA--;
                    if(manager.enemyCntA <= 0)
                    {
                        manager.enemyCntA = 0;
                    }
                    break;

                case Type.B:
                    manager.enemyCntB--;
                    if(manager.enemyCntB <= 0)
                    {
                        manager.enemyCntB = 0;
                    }
                    break;

                case Type.C:
                    manager.enemyCntC--;
                    if (manager.enemyCntC <= 0)
                    {
                        manager.enemyCntC = 0;
                    }
                    break;

                case Type.D:
                    manager.enemyCntD--;
                    if (manager.enemyCntD <= 0)
                    {
                        manager.enemyCntD = 0;
                    }
                    break;
                }

                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                
                Destroy(gameObject, 4);
                break;
            }
        }
    }

}
