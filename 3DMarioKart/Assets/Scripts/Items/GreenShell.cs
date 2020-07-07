using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS script is for shells moving around the track shot by opponents or player

public class GreenShell : MonoBehaviour
{
    private SphereCollider sphereCollider;
    private Rigidbody rb;

    public Vector3 myVelocity;
    [HideInInspector]
    public float lifetime = 0;

    [HideInInspector]
    public string who_threw_shell;

    bool grounded = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();



    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        groundNormalRotation();


    }

    private void Move()
    {
        myVelocity.y = rb.velocity.y;
        rb.velocity = myVelocity;

        
        Vector3 vel = transform.InverseTransformDirection(rb.velocity);
        vel.x *= 1;
        vel.y /= 5;
        vel.z *= 1;
        rb.velocity = transform.TransformDirection(vel);
        

        lifetime += Time.deltaTime;

    }
    void groundNormalRotation()
    {
        //ground normal rotation
        Ray ground = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ground, out hit, 10))
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, 5f * Time.deltaTime);
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Ground" && collision.gameObject.tag != "Dirt")
        {
            if (collision.gameObject.tag == "Shell")
            {
                destroyShell();
            }
            if (collision.gameObject.tag == "Banana")
            {
                destroyShell();
                Destroy(collision.gameObject);
            }


            if (collision.gameObject.name.Equals(who_threw_shell)) //wait 0.5 second before detecting collisions with person 
            {
                if (lifetime > 0.5f)
                {
                    destroyShell();

                }
                else
                {
                    Physics.IgnoreCollision(collision.collider, sphereCollider);
                }
            }
            else
            {
                if(collision.gameObject.tag != "GliderPanel")
                {
                    Vector3 oldvel = rb.velocity;
                    myVelocity = Vector3.Reflect(myVelocity, collision.contacts[0].normal);
                    myVelocity = new Vector3(myVelocity.x, oldvel.y, myVelocity.z);

                    if (lifetime > 20)
                    {
                        destroyShell();
                    }
                }
                
            }
            
            //HITTING OPPONENTS
            if(collision.gameObject.tag == "Opponent") 
            {
                collision.gameObject.GetComponent<OpponentItemManager>().hitByShell(); //the opponent has the function that does all this work
                if (who_threw_shell == "Player")
                {
                    GameObject.Find("Player").GetComponent<Player>().Driver.SetTrigger("HitItem");
                }
                destroyShell();
                
            }
            

        }
                
        
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag != "Ground" || collision.gameObject.tag != "Dirt")
        {
            grounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag != "Ground" || collision.gameObject.tag != "Dirt")
        {
            grounded = false;
        }
    }

    private void destroyShell()
    {
        int x = transform.GetChild(0).childCount; //particle systems
        for (int i = 0; i < x; i++)
        {
            transform.GetChild(0).GetChild(i).GetComponent<ParticleSystem>().Play();
        }
        transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        sphereCollider.enabled = false;
        rb.isKinematic = true;

        

        Destroy(gameObject, 3);

    }
}
