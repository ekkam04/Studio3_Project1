using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using Ekkam;

public class TileManager : MonoBehaviour
{
    public bool crumbleTiles = false;

    [SerializeField] GameObject tileUndamaged;
    [SerializeField] GameObject tileSlightlyDamaged;

    [SerializeField] GameObject tileVeryDamaged;
    [SerializeField] GameObject[] tileVeryDamagedPieces;

    List<Player> playersOnTile = new List<Player>();

    bool playerIsOnTile = false;
    bool isCrumbling = false;
    float timeSincePlayerSteppedOnTile = 0f;

    MeshCollider meshCollider;

    void Start()
    {
        tileVeryDamaged.SetActive(false);
        tileSlightlyDamaged.SetActive(false);
        tileUndamaged.SetActive(true);

        // Rotate hexagon tile randomly to make the cracks look more natural
        transform.Rotate(0, 0, Random.Range(0, 6) * 60);

        foreach (MeshCollider collider in GetComponentsInChildren<MeshCollider>())
        {
            if (collider.name == "Collider")
            {
                meshCollider = collider;
            }
        }
    }

    void Update()
    {
        if (!crumbleTiles) return;
        if ((playerIsOnTile || playersOnTile.Count > 0) && !isCrumbling)
        {
            timeSincePlayerSteppedOnTile += Time.deltaTime;
            if (timeSincePlayerSteppedOnTile >= 1f)
            {
                DamageTile();
                timeSincePlayerSteppedOnTile = 0f;
            }
        }
        else
        {
            timeSincePlayerSteppedOnTile = 0f;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // if collision contact is with child called "Collider", continue, else return
        foreach (ContactPoint collider in other.contacts)
        {
            if (collider.thisCollider.name != "Collider")
            {
                return;
            }
        }

        if (!meshCollider.enabled) return;
        if(other.gameObject.tag == "Player")
        {
            playerIsOnTile = true;
            playersOnTile.Add(other.gameObject.GetComponent<Player>());
            if (!crumbleTiles) return;
            DamageTile();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!meshCollider.enabled) return;
        if (other.gameObject.tag == "Player")
        {
            Player player = other.gameObject.GetComponent<Player>();
            playerIsOnTile = false;
            playersOnTile.Remove(player);

            
        }
    }

    async void DamageTile()
    {
        if (tileUndamaged.activeSelf)
        {
            tileUndamaged.SetActive(false);
            tileSlightlyDamaged.SetActive(true);
            
        }
        else if (tileSlightlyDamaged.activeSelf)
        {
            tileSlightlyDamaged.SetActive(false);
            tileVeryDamaged.SetActive(true);
            
        }
        else if (tileVeryDamaged.activeSelf)
        {
            

            // jitter the tile
            isCrumbling = true;
            for (int i = 0; i < 30; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
                tileVeryDamaged.transform.position += jitter;
                await Task.Delay(20);
                tileVeryDamaged.transform.position -= jitter;
            }

           
            

            meshCollider.enabled = false;
            GetComponent<MeshCollider>().enabled = false;
            foreach (GameObject piece in tileVeryDamagedPieces)
            {
                // Add rigidbody to piece
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.AddExplosionForce(100f, transform.position, 10f);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }
            
            Invoke("HideTile", 10f);
        }
    }

    void HideTile()
    {
        gameObject.SetActive(false);
    }
}
