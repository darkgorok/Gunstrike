using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SetLayerFollowPlayer : MonoBehaviour
{
    private readonly List<SpriteRenderer> listSpriteRenderer = new List<SpriteRenderer>();
    private readonly List<ParticleSystemRenderer> listParticle = new List<ParticleSystemRenderer>();
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        SpriteRenderer selfSpriteRenderer = GetComponent<SpriteRenderer>();
        if (selfSpriteRenderer)
            listSpriteRenderer.Add(selfSpriteRenderer);

        ParticleSystemRenderer selfParticle = GetComponent<ParticleSystemRenderer>();
        if (selfParticle)
            listParticle.Add(selfParticle);

        listSpriteRenderer.AddRange(transform.GetComponentsInChildren<SpriteRenderer>());
        listParticle.AddRange(transform.GetComponentsInChildren<ParticleSystemRenderer>());
    }

    private void Update()
    {
        if (gameSession?.Player == null)
            return;

        string sortingLayerName = LayerMask.LayerToName(gameSession.Player.gameObject.layer);
        foreach (SpriteRenderer spriteRenderer in listSpriteRenderer)
        {
            if (spriteRenderer != null)
                spriteRenderer.sortingLayerName = sortingLayerName;
        }

        foreach (ParticleSystemRenderer particleRenderer in listParticle)
        {
            if (particleRenderer != null)
                particleRenderer.sortingLayerName = sortingLayerName;
        }
    }
}
