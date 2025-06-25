using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// Ends the game on collision, forcing a lose state.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Collider))]
    public class Obstacle : Spawnable
    {
        const string k_PlayerTag = "Player";
        
        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(k_PlayerTag))
            {
                GameManager.Instance.Lose();
                StartCoroutine(DestroyObstacleAfterDelay(0.3f));
            }

        }

        private IEnumerator DestroyObstacleAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject); // Àå¾Ö¹° ÆÄ±«
        }
    }
}