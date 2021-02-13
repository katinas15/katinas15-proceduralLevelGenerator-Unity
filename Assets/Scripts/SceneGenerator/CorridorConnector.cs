using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorConnector : MonoBehaviour
{
    [SerializeField] Connector start;
    [SerializeField] Connector end;
    [SerializeField] SceneObject cyclicConnectionPrefab;

    List<Room> generatedRooms = new List<Room>();
    List<Corridor> generatedCorridors = new List<Corridor>();
    LayerMask sceneLayerMask;

    Vector2Int[] directions = new Vector2Int[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
        
    Queue<SceneObject> queue = new Queue<SceneObject>();
        WaitForSeconds startup =  new WaitForSeconds(1);
        WaitForFixedUpdate fixedUpdateInterval = new WaitForFixedUpdate();

    
    void Start() {
        sceneLayerMask = LayerMask.GetMask("SceneColliders");
        StartCoroutine("PathFinder");
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {

            StartCoroutine("PathFinder");
        }   
    }



    // void ConnectEmptyConnectors(){
    //     foreach(Connector corridorConnector in availableCorridorConnectors){
    //         foreach(Connector roomConnector in availableRoomConnectors){
    //             float distanceBetweenConnectors = Vector3.Distance(
    //                 corridorConnector.transform.position, 
    //                 roomConnector.transform.position);

    //             if(distanceBetweenConnectors <= cyclicConnectionRange){
    //                 PathFinder(corridorConnector, roomConnector);
    //             }
    //         }
    //     }
    // }

    IEnumerator PathFinder(){
        Connector corridorConnector = start;
        Connector roomConnector = end;
        SceneObject pathBlock = Instantiate(cyclicConnectionPrefab);
        pathBlock.transform.position = corridorConnector.transform.position;
        queue.Enqueue(pathBlock);

        // while(queue.Count > 0){
        for(int i = 0; i < 50; i++){
            var searchCenter = queue.Dequeue();
            yield return StartCoroutine("ExploreNeighbours", searchCenter.transform);
            StopIfEndFound(searchCenter.transform, roomConnector.transform);
        }

        StopCoroutine("PathFinder");
    }

    IEnumerator ExploreNeighbours(Transform from){
        foreach(Vector2Int direction in directions){
            SceneObject pathBlock = Instantiate(cyclicConnectionPrefab);
            pathBlock.transform.position = new Vector3(
                from.position.x + direction.x * from.localScale.x,
                from.transform.position.y,
                from.transform.position.z + direction.y * from.localScale.z);

            yield return fixedUpdateInterval;
            if(CheckOverlap(pathBlock)){
                Destroy(pathBlock.gameObject);
            } else {
                queue.Enqueue(pathBlock);
            }
        }
        StopCoroutine("ExploreNeighbours");
    }

   


    void StopIfEndFound(Transform current, Transform end){
        float distance = Vector3.Distance(
                    current.position, 
                    end.position);
        if(distance <= 1f){
            print("stop");
        }
    }







    // void CleanUp(){
    //     generatedCorridors.Clear();
    //     generatedRooms.Clear();
    //     availableRoomConnectors.Clear();
    //     availableCorridorConnectors.Clear();
    //     foreach (Transform child in this.transform) {
    //         GameObject.Destroy(child.gameObject);
    //     }
    // }



     bool CheckOverlap(SceneObject sceneObject){
        List<BoxCollider> objectColliders = sceneObject.getColliders();
        foreach(BoxCollider boxCollider in objectColliders){
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(-0.1f);

            Collider[] colliders = Physics.OverlapBox(boxCollider.transform.position, bounds.size / 2, boxCollider.transform.rotation, sceneLayerMask);
            if(colliders.Length > 0){
                foreach(Collider c in colliders){
                    if(c.transform.parent.gameObject.transform.parent.gameObject.Equals(sceneObject.gameObject)){
                        continue;
                    } else {
                        return true;
                    }
                }
            }
        }

        return false;
    }

}
