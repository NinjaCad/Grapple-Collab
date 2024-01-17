using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : PlayerSystem
{
    GameObject lastCheckpoint;
    
    bool canMove = true;
    
    void Update()
    {
        if (!canMove) { return; }
        
        RaycastHit2D boxcast = Physics2D.BoxCast(transform.position, player.data.checkpointCheckSize, 0f, Vector2.zero, 0f, player.data.checkpointLayer);
        if (boxcast.collider != null) { lastCheckpoint = boxcast.collider.gameObject; }
        
        if (Physics2D.OverlapBox(transform.position, player.data.hazardCheckSize, 0f, player.data.hazardLayer)) { StartCoroutine(OnDeath()); }
    }

    IEnumerator OnDeath()
    {
        canMove = false;
        player.events.OnDeath?.Invoke();

        yield return new WaitForSeconds(0.5f);

        if (lastCheckpoint != null) { transform.position = lastCheckpoint.transform.position; }
        else { transform.position = Vector2.zero; }
        
        player.events.OnRespawn?.Invoke();
        canMove = true;
    }
}
