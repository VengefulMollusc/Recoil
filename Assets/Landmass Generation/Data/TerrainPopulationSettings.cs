using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainPopulationSettings : UpdatableData
{
    [Range(1, 5)]
    public int populationIndexStep;

    public List<PopulatableObject> populatableObjects;
}
