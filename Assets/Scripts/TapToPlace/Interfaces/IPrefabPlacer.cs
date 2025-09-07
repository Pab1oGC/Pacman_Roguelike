using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPrefabPlacer 
{
    GameObject Place(GameObject prefab, GameObject current, Transform anchor);
}
