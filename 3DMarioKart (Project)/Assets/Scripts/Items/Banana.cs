using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody rb;

    public float throwForceUp;
    public float throwForceForward;

    public float lifetime;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        lifetime += Time.deltaTime;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag != "fence")
        {
            groundNormalRotation();

            rb.velocity = Vector3.zero;
            GetComponent<Animator>().SetTrigger("LandGround");
        }
    }

    public void Move()
    {
        
    }

    public void Banana_thrown(float extraForward){
        rb.AddForce(transform.up * throwForceUp * Time.deltaTime, ForceMode.Impulse);
        rb.AddForce(-transform.forward * (throwForceForward + extraForward) * Time.deltaTime, ForceMode.Impulse);
    }


    void groundNormalRotation()
    {
        //ground normal rotation
        Ray ground = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ground, out hit, 10))
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, 9f * Time.deltaTime);
            Debug.DrawRay(hit.point, hit.normal, Color.white, 20f);
        }
    }
}
