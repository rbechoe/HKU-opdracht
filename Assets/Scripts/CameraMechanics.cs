using UnityEngine;

public class CameraMechanics : MonoBehaviour
{
    GameMechanics GM;
    GameData GD;

    void Start()
    {
        GM = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMechanics>();
        GD = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameData>();
    }

    void Update()
    {
        if (GM.started)
        {
            transform.position = GD.players[GM.player_cur].transform.position + (Vector3.up * 5);
            transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            transform.LookAt(GD.players[GM.player_cur].transform.position);
        }
    }
}
