using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2.5D character controller 
public class PlayerController : MonoBehaviour {
    
    CharacterController myCharacterController;
    Vector3 moveDelta = new Vector3();
    Vector3 groundCheckPosition = new Vector3();
    [SerializeField] 
    bool jumping, grounded, walled;
    float jumpTimer,x,z;
    int jumpCount;
    //float groundCheckDistance;
    [Range(0f, 1f)]
    public float acceleration = 0.5f;
    [Range(0f, 1f)]
    public float decceleration = 0.5f;
    [Range(1f, 100f)]
    public float rotateSpeed = 20f;
    [Range(1f, 30f)]
    public float maxSpeed = 5f;
    [Range(1f, 10f)]
    public float jumpSpeed = 5f;
    [Range(0f, 2f)]
    public float gravity = 1f;
    [Range(10f, 50f)]
    public float terminalVelocity = 20f;
    [Range(0.1f, 5f)]
    public float maxJumpTime = 0.3f;
    [Range(0f, 2f)]
    public float aerialControl = 0.5f;
    public bool canDoubleJump = true;
    public string pushableTag = "Pushable";
    [Range(1f, 5f)]
    public float pushPower = 1.5f;
    public string wallTag = "Wall";
    [Range(0f, 50f)]
    public float wallJumpSpeed = 12f;
    [Range(0f, 10f)]
    public float wallSlideSpeed = 2f;

    //public float groundCheckRadius = 0.5f, skinWidth = 0.1f;
    //public LayerMask groundLayer;

    // Use this for initialization
    void Start ()
    {
        myCharacterController = GetComponent<CharacterController>();
	}

    // Update is called once per frame
    void Update ()
    {
        // Custom ground check **********************************************************************
        //groundCheckDistance = GetComponent<CapsuleCollider>().bounds.extents.y + skinWidth;
        //groundCheckPosition = transform.position + Vector3.down * groundCheckDistance;
        //isGrounded = Physics.CheckSphere(groundCheckPosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        grounded = myCharacterController.isGrounded;

        // Handle running, acceleration and decceleration
        x = Input.GetAxis("Horizontal");

        // Look the intended movement direction
        if(x>0.1f)
        {
            //transform.Rotate(transform.right);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.right), rotateSpeed);

        }
        else if(x<-0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-Vector3.right), rotateSpeed);
        }

        // Accel and decell
        if (x > 0.1f || x < -0.1f)
        {
            if (grounded)
            {
                moveDelta.x += x * acceleration;
            }
            else
            {
                moveDelta.x += x * acceleration * aerialControl;
            }
            if(!walled)
            moveDelta.x = Mathf.Clamp(moveDelta.x, -maxSpeed, maxSpeed);
        }
        else
        {
            if (grounded)
            {
                moveDelta.x = Mathf.MoveTowards(moveDelta.x, 0, decceleration);
            }
            else
            {
                moveDelta.x = Mathf.MoveTowards(moveDelta.x, 0, decceleration * aerialControl);
            }
        }

        // Use Z for 3d movement only *******************************
        //z = Input.GetAxis("Horizontal");

        // Do ground stuff 
        if (grounded)
        {
            Land();
        }

        // Check for jumping
        if (Input.GetButtonDown("Jump") && jumpCount <= 1)
        {
            Jump();
        }
        else if(Input.GetButtonUp("Jump"))
        {
            StopJumping();
        }

        // If in the air and not jumping, apply gravity otherwise do jumping stuff
        if (!grounded && !jumping)
        {
            moveDelta.y -= gravity;
            moveDelta.y = Mathf.Clamp(moveDelta.y, -terminalVelocity, terminalVelocity);
        }
        else if (jumping)
        {
            jumpTimer += Time.deltaTime;
            moveDelta.y = jumpSpeed ;

            if(jumpTimer>=maxJumpTime)
            {
                StopJumping();
            }
        }

        Debug.Log(moveDelta);
        myCharacterController.Move(moveDelta * Time.deltaTime);
        //moveDelta = transform.TransformDirection(moveDelta); // Use for worldspace movement

        // For 2.5D Character Controller ******************************
        Vector3 fixed2dPosition = transform.position;
        if (transform.position.z != 0)
        {
            fixed2dPosition.z = 0;
            transform.position = fixed2dPosition;
        }
    }

    private void FixedUpdate()
    {

    }

    public void Jump()
    {
        jumpTimer = 0f;
        ++jumpCount;
        jumping = true;
    }

    public void StopJumping()
    {
        jumping = false;
        walled = false;
    }

    public void Land()
    {
        jumpCount = 0;
        jumpTimer = 0f;

        // If on ground but falling, reset y speed
        if (moveDelta.y < 0)
        {
            moveDelta.y = 0f;
        }
    }

    // Check for character controller collisions
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Handle pushing        
        if (hit.gameObject.CompareTag(pushableTag) && grounded)
        {
            Debug.Log("Character Controller hit Pushable");

            Rigidbody rb = hit.collider.attachedRigidbody;

            if (rb != null && !rb.isKinematic)
            {
                Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, 0);
                rb.velocity = pushDirection * pushPower;
            }
            else
            {
                Debug.Log("Pushable does not have a rigidbody or collider attached");
            }
        }

        // Handle walling
        if(hit.gameObject.CompareTag(wallTag) && hit.normal.y <0.2f)
        {
            walled = true;
            moveDelta.x = 0;
            moveDelta.y = Mathf.Clamp(moveDelta.y, -wallSlideSpeed, terminalVelocity);

            if(!grounded && Input.GetButtonDown("Jump"))
            {
                moveDelta = hit.normal * wallJumpSpeed;
                moveDelta.y = jumpSpeed;
                Debug.Log(moveDelta + "walling");
                Jump();
            }
        }
        else
        {
            walled = false;
        }
    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(groundCheckPosition, groundCheckRadius);
    //}
}
