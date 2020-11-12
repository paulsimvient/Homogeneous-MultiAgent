using UnityEngine;
using System.Collections;

public class LPTDestroyTimer : MonoBehaviour
{
    [SerializeField]
    private bool _runOnStart = false;

    [SerializeField]
    private float _lifeTime = 3f;


    private void Start()
    {
        if (_runOnStart)
            Run(_lifeTime);
    }

    public void Run(float lifeTime)
    {
        _lifeTime = lifeTime;
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(_lifeTime);

        if (gameObject != null)
            Destroy(gameObject);
    }
}
