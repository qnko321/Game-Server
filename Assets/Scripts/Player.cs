using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    private bool isSpawned = true;
    public GameObject obj;
    public PlayerController controller;

    public bool IsSpawned 
    {
        get { return isSpawned; }
        set { isSpawned = value; if (!value) { Destroy(gameObject); }}
    }    

    public void Populate(int _id, string _username)
    {
        id = _id;
        username = _username;
        obj = gameObject;
    }
}