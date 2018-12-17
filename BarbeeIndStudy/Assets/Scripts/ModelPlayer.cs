using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ModelPlayer : NetworkBehaviour
{

    public bool keyboard = true;
    private PlayerInputFactory input;
    Vector2 movement = Vector2.zero;
    readonly float walkSpeed = 8;

    bool canJump = true;
    public bool sameJump = false;
    float jumpStrength = 6;
    public float remainingJumpPower = 0;

    public bool dead = false;
    bool respawning = false;
    const int maxHealth = 100;
    int health = 100;
    float weaponCharge = 0;
    private const int weaponMaxCharge = 75;

    Rigidbody rb;
    public Transform gun;
    public Transform head;
    public Transform barrel;
    public GameObject chargeMeter;

    public SkinnedMeshRenderer[] playerModel;
    public Transform[] armBones;
    public Transform hand;
    Animator animator;
    Vector3 lastPosition;
    float stillTime = 0;
    public GameObject[] hitbox;
    public GameObject myCollider;
    public GameObject myCamera;

    LayerMask jumpMask;

    public GameObject laserFab;
    public GameObject deathFab;

    Texture2D crosshair;
    Texture2D healthBack;
    Texture2D healthBar;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        jumpMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("EnemyCollider");

        //Set up Input
        if (keyboard)
        {
            input = new KeyboardMousePlayerInputFactory();
        }
        else
        {
            input = new GamepadPlayerInputFactory();
        }
        input.Init();

        //Turn off playermodel for localplayer, make others into enemies.
        if (isLocalPlayer)
        {
            foreach (SkinnedMeshRenderer mesh in playerModel)
            {
                mesh.enabled = false;
            }
            transform.position = FindObjectOfType<SpawnPoints>().GetRandomSpawn();
        }
        else
        {
            myCamera.SetActive(false);
            foreach (GameObject box in hitbox)
            {
                box.layer = LayerMask.NameToLayer("EnemyHitbox");
                box.tag = "Enemy";
            }
            myCollider.layer = LayerMask.NameToLayer("EnemyCollider");
        }

        //Set up crosshair texture
        crosshair = new Texture2D(1, 1);
        crosshair.SetPixel(0, 0, Color.black);
        crosshair.wrapMode = TextureWrapMode.Repeat;
        crosshair.Apply();

        //Set up health bar texture
        healthBack = new Texture2D(1, 1);
        healthBack.SetPixel(0, 0, Color.gray);
        healthBack.wrapMode = TextureWrapMode.Repeat;
        healthBack.Apply();

        healthBar = new Texture2D(1, 1);
        healthBar.SetPixel(0, 0, Color.red);
        healthBar.wrapMode = TextureWrapMode.Repeat;
        healthBar.Apply();

    }

    void Update()
    {

        if (!isLocalPlayer)
        {
            return; //do nothing
        }

        if (dead)
        {
            if (respawning)
            {
                return;
            }
            respawning = true;
            StartCoroutine("WaitToSpawn");
            return;
        }

        if (respawning)
        {
            respawning = false;
            foreach (SkinnedMeshRenderer mesh in playerModel)
            {
                mesh.enabled = false;
            }
        }

        Vector2 walk = input.WalkVec();
        
        transform.Translate(walkSpeed * Time.deltaTime * new Vector3(walk.x, 0, walk.y));
        Vector2 look = input.LookVec();
        transform.Rotate(new Vector3(0, look.x, 0));
        head.Rotate(new Vector3(-look.y, 0, 0));

        

        //Check below feet to see if the player should be able to jump
        canJump = Physics.CheckBox(transform.position, new Vector3(0.3f, 0.1f, 0.3f), Quaternion.LookRotation(transform.forward, Vector3.up), jumpMask, QueryTriggerInteraction.Ignore);
        if (canJump && (input.GetJumpDown() || (input.GetJump() && !sameJump))) //Initial jump
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpStrength, rb.velocity.z);
            remainingJumpPower = 4;
            sameJump = true;
        }
        else if (input.GetJump() && remainingJumpPower > 0) //bonus height from holding space
        {
            rb.AddForce(new Vector3(0, Mathf.Min(8 * Time.deltaTime, remainingJumpPower), 0), ForceMode.VelocityChange);
            remainingJumpPower -= 8 * Time.deltaTime;
        }
        else if (!input.GetJump()) //Once they let go of space, remove any option for more bonus height
        {
            remainingJumpPower = 0;
        }

        if (!canJump) //Used to allow holding space to keep jumping
        {
            sameJump = false;
        }

        //Firing
        if (input.GetFireUp())
        {
            Fire();
        }
        else if (input.GetFire())
        {
            float mod = 1 + (2 * weaponCharge) / 100;
            weaponCharge = weaponCharge + (20 * Time.deltaTime * mod);
            if (weaponCharge >= weaponMaxCharge)
            {
                weaponCharge = weaponMaxCharge;
                Fire();
            }
        }
        if (input.GetFireDown())
        {
            weaponCharge = 10;
        }

        //Weapon Visual
        CmdSyncCharge(this.gameObject, weaponCharge / weaponMaxCharge);




    }

    void LateUpdate()
    {
        if (dead)
        {
            return; //do nothing
        }

        if (Vector3.ProjectOnPlane(lastPosition - transform.position, Vector3.up).magnitude <= 0.01f)
        {
            if (isLocalPlayer)
            {
                animator.SetBool("run", false);
            }
            else
            {
                stillTime += Time.deltaTime;
                if (stillTime > 1 / 15f)
                {
                    animator.SetBool("run", false);
                }
            }
            
        }
        else
        {
            stillTime = 0;
            animator.SetBool("run", true);
        }

        foreach (Transform bone in armBones)
        {
            bone.LookAt(bone.position + head.forward, Vector3.up);
            gun.transform.position = hand.transform.position;
            gun.LookAt(gun.position + head.forward, Vector3.up);
        }
        lastPosition = transform.position;
    }

    [Command]
    void CmdSyncCharge(GameObject bar, float scale)
    {
        RpcShowCharge(this.gameObject, scale);
    }

    [ClientRpc]
    void RpcShowCharge(GameObject bar, float scale)
    {
        bar.GetComponent<ModelPlayer>().chargeMeter.transform.localScale = new Vector3(1, 1, scale);
    }

    public void OnGUI()
    {
        if (!isLocalPlayer)
        {
            return; //Do nothing
        }
        else
        {
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 1, Screen.height / 2 - 4), new Vector2(2, 8)), crosshair);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 4, Screen.height / 2 - 1), new Vector2(8, 2)), crosshair);

            int barWidth = Screen.width / 3;
            float healthRatio = ((float)health) / maxHealth;
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 3, 10), new Vector2(barWidth, 16)), healthBack);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 3, 10), new Vector2(Mathf.RoundToInt(barWidth * healthRatio), 16)), healthBar);
        }
    }

    void Fire()
    {
        RaycastHit laserHit;
        Vector3 endpoint;
        LayerMask rayMask = ~(LayerMask.GetMask("Hitbox") | LayerMask.GetMask("PlayerCollider") | LayerMask.GetMask("EnemyCollider"));
        if (Physics.Raycast(new Ray(head.transform.position, head.transform.forward), out laserHit, 1000, rayMask))
        {
            Debug.Log(laserHit.collider.name);
            if (laserHit.collider.CompareTag("Enemy"))
            {
                CmdHurt(laserHit.collider.GetComponentInParent<ModelPlayer>().gameObject, Mathf.CeilToInt(weaponCharge));
            }
            endpoint = laserHit.point;
        }
        else
        {
            endpoint = head.transform.position + 100 * head.transform.forward;
        }
        weaponCharge = 0;

        CmdSpawnLaser(barrel.transform.position, endpoint);
    }

    [Command]
    void CmdSpawnLaser(Vector3 origin, Vector3 destination)
    {
        GameObject laser = Instantiate(laserFab);

        laser.transform.position = origin;
        laser.transform.LookAt(destination);


        NetworkServer.Spawn(laser);
        RpcPointLaser(laser, Vector3.Distance(origin, destination));

        Destroy(laser, 0.5f);
    }

    [ClientRpc]
    void RpcPointLaser(GameObject laser, float scale)
    {
        laser.transform.localScale = new Vector3(1, 1, scale);
    }

    [Command]
    public void CmdHurt(GameObject player, int damage)
    {
        RpcSyncDamage(player, damage);
    }

    [ClientRpc]
    public void RpcSyncDamage(GameObject player, int damage)
    {
        ModelPlayer other = player.GetComponent<ModelPlayer>();
        other.health -= damage;
        if (other.health <= 0)
        {
            GameObject death = Instantiate(deathFab, other.head.position, other.transform.rotation);
            Destroy(death, 4);

            other.myCollider.SetActive(false);
            other.rb.velocity = Vector3.zero;
            other.rb.useGravity = false;

            foreach (SkinnedMeshRenderer m in other.playerModel)
            {
                m.enabled = false;
            }

            other.gun.gameObject.SetActive(false);

            foreach (GameObject h in other.hitbox)
            {
                h.SetActive(false);
            }

            other.dead = true;
        }
    }

    IEnumerator WaitToSpawn()
    {
        yield return new WaitForSeconds(4);
        CmdRespawn(this.gameObject);
    }

    [Command]
    void CmdRespawn(GameObject player)
    {
        RpcSyncRespawn(player, FindObjectOfType<SpawnPoints>().GetRandomSpawn());
    }

    [ClientRpc]
    void RpcSyncRespawn(GameObject player, Vector3 pos)
    {
        ModelPlayer playerScript = player.GetComponent<ModelPlayer>();
        playerScript.myCollider.SetActive(true);
        playerScript.rb.useGravity = true;

        foreach (SkinnedMeshRenderer m in playerScript.playerModel)
        {
            m.enabled = true;
        }

        playerScript.gun.gameObject.SetActive(true);

        foreach (GameObject h in playerScript.hitbox)
        {
            h.SetActive(true);
        }

        playerScript.dead = false;
        playerScript.health = maxHealth;

        player.transform.position = pos;
    }
}
