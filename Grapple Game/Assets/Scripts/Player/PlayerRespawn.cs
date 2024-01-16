using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : PlayerSystem
{
    GameObject lastCheckpoint;
    
    bool inDeathSequence;
    
    void Update()
    {
        if (inDeathSequence) { return; }
        
        RaycastHit2D boxcast = Physics2D.BoxCast(transform.position, player.ID.data.checkpointCheckSize, 0f, Vector2.zero, 0f, player.ID.data.checkpointLayer);
        if (boxcast.collider != null) { lastCheckpoint = boxcast.collider.gameObject; }
        
        if (Physics2D.OverlapBox(transform.position, player.ID.data.hazardCheckSize, 0f, player.ID.data.hazardLayer)) { StartCoroutine(OnDeath()); }
    }

    IEnumerator OnDeath()
    {
        inDeathSequence = true;
        player.ID.events.OnDeath?.Invoke();

        yield return new WaitForSeconds(1.0f);

        if (lastCheckpoint != null) { transform.position = lastCheckpoint.transform.position; }
        else { transform.position = Vector2.zero; }
        
        player.ID.events.OnRespawn?.Invoke();
        inDeathSequence = false;
    }
}
