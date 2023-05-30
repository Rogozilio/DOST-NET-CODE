using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

public class NetworkDataPersistens : MonoBehaviour
{
    private EntityManager _entityManager;
    private EntityQuery _entityQuery;
    // Start is called before the first frame update
    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _entityQuery = _entityManager.CreateEntityQuery(typeof(Cube));
    }

    // Update is called once per frame
    void Update()
    {
        var entities = _entityQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            _entityManager.SetComponentData(entity, new Cube(){Position = transform.position});
            Debug.Log(_entityManager.GetName(entity));
        }

        entities.Dispose();
    }
}
