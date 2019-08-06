using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public bool IsGrounded;
    public bool IsSprinting;
    public float WalkSpeed = 3f;
    public float SprintSpeed = 6f;
    public float JumpForce = 5f;
    public float Gravity = -9.8f;
    public Transform HighlightBlock;
    public Transform PlaceBlock;
    public float CheckIncrement = 0.1f;
    public float Reach = 8f;

    public float PlayerWidth = 0.15f;


    private Transform Camera;
    private World world;
    private float Horizontal;
    private float Vertical;
    private float MouseHorizontal;
    private float MouseVertical;
    private float PlayerHeight = 2f;
    private Vector3 velocity;
    private float VerticalMomentum;
    private bool JumpRequest;

    public byte SelectedBlockIndex = 1;



    private void Start()
    {
        Camera = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log(world.BlockTypes[SelectedBlockIndex].BlockName + " Selected Block");
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if (JumpRequest)
        {
            Jump();
        }

        transform.Rotate(Vector3.up * MouseHorizontal);

        Camera.Rotate(Vector3.right * -MouseVertical);

        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInput();
        PlaceCursorBlock();

    }

    private void CalculateVelocity()
    {

        //Affect vertical momemtum with gravity
        if(VerticalMomentum > Gravity)
        {
            VerticalMomentum += Time.fixedDeltaTime * Gravity;
        }

        //sprinting
        if (IsSprinting)
            velocity = ((transform.forward * Vertical) + (transform.right * Horizontal)) * Time.fixedDeltaTime * SprintSpeed;
        else
            velocity = ((transform.forward * Vertical) + (transform.right * Horizontal)) * Time.fixedDeltaTime * WalkSpeed;
        //Apply vertical momentum 

        velocity += Vector3.up * VerticalMomentum * Time.fixedDeltaTime;

        if((velocity.z > 0 && Front) || (velocity.z < 0 && Back))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && Right) || (velocity.x < 0 && Left))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }

    }

    private void GetPlayerInput()
    {
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");

        MouseHorizontal = Input.GetAxis("Mouse X");
        MouseVertical = Input.GetAxis("Mouse Y");

        if(Input.GetButtonDown("Sprint"))
        {
            IsSprinting = true;
        }
        if(Input.GetButtonUp("Sprint"))
        {
            IsSprinting = false;
        }

        if(IsGrounded && Input.GetButtonDown("Jump"))
        {
            JumpRequest = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {

            if (scroll > 0)
                SelectedBlockIndex++;
            else
                SelectedBlockIndex--;

            var length = world.BlockTypes.Length - 1;

            if (SelectedBlockIndex > (byte)(length))
                SelectedBlockIndex = 1;

            if (SelectedBlockIndex < 1)
                SelectedBlockIndex = (byte)(length);

            Debug.Log(world.BlockTypes[SelectedBlockIndex].BlockName + " Selected Block");
        }

        if(HighlightBlock.gameObject.activeSelf)
        {
            //destroy block
            if (Input.GetMouseButtonDown(0))
                world.GetChunkFromVector3(HighlightBlock.position).EditVoxel(HighlightBlock.position, 0);

            //create
            if (Input.GetMouseButtonDown(1))
                world.GetChunkFromVector3(PlaceBlock.position).EditVoxel(PlaceBlock.position, SelectedBlockIndex);
        }

    }

    public void PlaceCursorBlock()
    {
        float step = CheckIncrement;
        Vector3 lastPosition = new Vector3();

        while(step < Reach)
        {
            Vector3 pos = Camera.position + (Camera.forward * step);

            if(world.CheckForVoxel(pos))
            {
                HighlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                PlaceBlock.position = lastPosition;

                HighlightBlock.gameObject.SetActive(true);
                PlaceBlock.gameObject.SetActive(true);

                return;
            }
            else
            {
                lastPosition = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                step += CheckIncrement;
            }
        }
        HighlightBlock.gameObject.SetActive(false);
        PlaceBlock.gameObject.SetActive(false);

    }

    void Jump()
    {
        VerticalMomentum = JumpForce;
        IsGrounded = false;
        JumpRequest = false;
    }

    private float CheckDownSpeed(float DownSpeed)
    {

        if (
            world.CheckForVoxel(new Vector3(transform.position.x - PlayerWidth, transform.position.y + DownSpeed, transform.position.z - PlayerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + PlayerWidth, transform.position.y + DownSpeed, transform.position.z - PlayerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + PlayerWidth, transform.position.y + DownSpeed, transform.position.z + PlayerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - PlayerWidth, transform.position.y + DownSpeed, transform.position.z + PlayerWidth))
           )
        {
            IsGrounded = true;
            return 0;

        }
        else
        {
            IsGrounded = false;
            return DownSpeed;

        }

    }


    private float CheckUpSpeed(float UpSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - PlayerWidth, transform.position.y + PlayerHeight + UpSpeed, transform.position.z - PlayerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + PlayerWidth, transform.position.y + PlayerHeight + UpSpeed, transform.position.z - PlayerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + PlayerWidth, transform.position.y + PlayerHeight + UpSpeed, transform.position.z + PlayerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - PlayerWidth, transform.position.y + PlayerHeight + UpSpeed, transform.position.z + PlayerWidth)))
        {
            return 0;
        }
        else
        {
            return UpSpeed;
        }
    }

    public bool Front
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + PlayerWidth)) ||
              world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + PlayerWidth)))
            { return true; }
            else { return false; }
        }
    }

    public bool Back
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - PlayerWidth)) ||
              world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - PlayerWidth)))
            { return true; }
            else { return false; }
        }
    }

    public bool Left
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - PlayerWidth, transform.position.y, transform.position.z )) ||
              world.CheckForVoxel(new Vector3(transform.position.x - PlayerWidth, transform.position.y + 1f, transform.position.z )))
            { return true; }
            else { return false; }
        }
    }

    public bool Right
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + PlayerWidth, transform.position.y, transform.position.z)) ||
              world.CheckForVoxel(new Vector3(transform.position.x + PlayerWidth, transform.position.y + 1f, transform.position.z)))
            { return true; }
            else { return false; }
        }
    }
}
