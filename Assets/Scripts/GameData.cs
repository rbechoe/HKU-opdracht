using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameData : MonoBehaviour
{
    // good dome allows player to move forward x amount of tiles
    // bad dome forces the player back x amount of tiles
    // special dome asks if the player wants to take on a challenge, which rewards the player
    public List<GameObject> domes, domes_bad, domes_good, domes_special, players;
    public List<string> player_names;
    public List<int> player_position, player_rolls, player_tiles_left;

    public GameObject dome_model, dome_collection, player_a, player_b, player_c, input_amount, input_name, player_ui, game_ui;

    int trigger_dome_gap = 10, trigger_dome_offset = 5;

    int game_length = 100, player_amount = 1, chosen_model = 1, current_player = 0;

    GameMechanics GM;

    void Start()
    {
        GM = gameObject.GetComponent<GameMechanics>();
    }

    void Update()
    {

    }

    public void UpdateGameLength (int new_length)
    {
        game_length = new_length;
    }

    public void SetPlayerAmount ()
    {
        player_amount = int.Parse(input_amount.GetComponent<Text>().text);
    }

    public void SetModel (int type)
    {
        chosen_model = type;
        // 1 = cube, 2 = sphere, 3 = cylinder
    }

    public void SetPlayer ()
    {
        // increase current player amount, once all players have been created unlock start game button
        GameObject prefab = null;
        switch(chosen_model)
        {
            case 1:
            default:
                prefab = player_a;
                break;

            case 2:
                prefab = player_b;
                break;

            case 3:
                prefab = player_c;
                break;
        }
        GameObject player_model = Instantiate(prefab, new Vector3(0, 0.5f, 0), Quaternion.identity) as GameObject;
        player_model.GetComponent<Renderer>().material.color = new Color((Random.Range(0, 100) / 100f), (Random.Range(0, 100) / 100f), (Random.Range(0, 100) / 100f), 1);

        players.Add(player_model);
        player_names.Add(input_name.GetComponent<Text>().text);
        player_rolls.Add(0);
        player_tiles_left.Add(0);
        player_position.Add(0);

        current_player += 1; 
        if (current_player == player_amount)
        {
            // start game
            player_ui.SetActive(false);
            game_ui.SetActive(true);
            GenerateGame();
            GM.StartGame();
            GM.player_max = player_amount;
        }
    }

    public void GenerateGame()
    {
        StartCoroutine(InstantiateDomes(game_length));
    }

    IEnumerator InstantiateDomes (int domes_req)
    {
        int x = 0;
        int y = 0;
        int z = 0;

        // Generate the domes
        for (int i = 0; i < domes_req; i++)
        {
            // First set all the coordinates
            x += 2; // always move 2 tiles to the right in order to have some space between each tile
            bool go_up = (Random.Range(0, 4) >= 2) ? true : false; //"randomly" make a tile increase or decrease in height
            if (go_up)
                y += 1;
            else
                y -= 1;

            GameObject new_dome = Instantiate(dome_model, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            new_dome.name = "Dome " + i;
            new_dome.transform.parent = dome_collection.transform;
            domes.Add(new_dome);
        }

        // Pick special domes; eg. good, bad, challenge
        int special_domes = domes_req / trigger_dome_gap;
        for (int i = 0; i < special_domes; i++)
        {
            int dome_type = Random.Range(0, 3);
            GameObject temp_dome = domes[i * trigger_dome_gap + trigger_dome_offset]; // +5 so it doesn't start at first tile
            switch (dome_type)
            {
                default: // bad dome
                    domes_bad.Add(temp_dome);
                    temp_dome.name = temp_dome.name + " (bad)";
                    temp_dome.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case 1: // good dome
                    domes_good.Add(temp_dome);
                    temp_dome.name = temp_dome.name + " (good)";
                    temp_dome.GetComponent<Renderer>().material.color = Color.green;
                    break;
                case 2: // special dome
                    domes_special.Add(temp_dome);
                    temp_dome.name = temp_dome.name + " (special)";
                    temp_dome.GetComponent<Renderer>().material.color = Color.blue;
                    break;
            }
        }

        // Set tiles per player
        for (int i = 0; i < player_tiles_left.Count; i++)
        {
            player_tiles_left[i] = domes.Count - 1;
        }

        yield return new WaitForEndOfFrame();
    }
}