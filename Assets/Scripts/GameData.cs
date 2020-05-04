using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameData : MonoBehaviour
{
    // good dome allows player to move forward x amount of tiles
    // bad dome forces the player back x amount of tiles
    // special dome asks if the player wants to take on a challenge, which rewards the player
    public List<GameObject> domes, domes_bad, domes_good, domes_special, players, props;
    public List<string> player_names;
    public List<int> player_position, player_rolls, player_tiles_left, dome_heights;

    public GameObject dome_model, dome_collection, surface_collection, prop_collection, player_a, player_b, player_c, input_amount, input_name, player_ui, game_ui;
    public GameObject surface_010, surface_011, surface_012, surface_110, surface_111, surface_112, surface_210, surface_211, surface_212;

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
            int next_tile = Random.Range(0, 4);
            if (next_tile <= 1)
                y += 1;
            else if (next_tile >= 3)
                y -= 1;

            dome_heights.Add(y);

            GameObject new_dome = Instantiate(dome_model, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            new_dome.name = "Dome " + i + " " + y;
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

        // Create environment
        string height_combi = "";
        GameObject spawn = null;
        int prop_cd = Random.Range(0,10);
        for (int i = 0; i < domes_req; i++)
        {
            /***
             * Height logic - each number represents a dome, value is height
             * 0 = down
             * 1 = middle
             * 2 = up
             * 
             * sample: 1 1 0
             * - - _
             * sample: 2 1 2
             * _   _
             *   -
             * sample: 0 1 2
             *     _
             * _ -
             * */

            // First calculate the height logic per layer
            height_combi = "";

            // decide first layer
            if (i == 0)
            {
                height_combi = height_combi + "1"; // starting dome so start at 1
            }
            else
            {
                if (dome_heights[i - 1] == dome_heights[i]) // same level
                    height_combi = height_combi + "1";
                else if (dome_heights[i - 1] > dome_heights[i]) // higher
                    height_combi = height_combi + "2";
                else // lower only remains
                    height_combi = height_combi + "0";
            }
            // second layer is always middle level
            height_combi = height_combi + "1";
            // lastly calculate the final layer
            if (i == (domes_req - 1))
            {
                height_combi = height_combi + "1"; // final dome so end at 1
            }
            else
            {
                if (dome_heights[i + 1] == dome_heights[i]) // same level
                    height_combi = height_combi + "1";
                else if (dome_heights[i + 1] > dome_heights[i]) // higher
                    height_combi = height_combi + "2";
                else // lower only remains
                    height_combi = height_combi + "0";
            }

            // Second we can use the created string to spawn the correct height block
            spawn = null;
            switch (height_combi)
            {
                case "010":
                    spawn = surface_010;
                    break;
                case "011":
                    spawn = surface_011;
                    break;
                case "012":
                    spawn = surface_012;
                    break;
                case "110":
                    spawn = surface_110;
                    break;
                case "111":
                default:
                    spawn = surface_111;
                    break;
                case "112":
                    spawn = surface_112;
                    break;
                case "210":
                    spawn = surface_210;
                    break;
                case "211":
                    spawn = surface_211;
                    break;
                case "212":
                    spawn = surface_212;
                    break;
            }

            // Finally create the object
            GameObject surface_obj = Instantiate(spawn, new Vector3(2 + (2 * i), dome_heights[i], z), Quaternion.identity) as GameObject;
            surface_obj.name = "Surface " + i + " " + height_combi;
            surface_obj.transform.parent = surface_collection.transform;
            surface_obj.transform.localEulerAngles = new Vector3(270, 180, 0); // fix object orientation

            // Randomly spawn props
            prop_cd -= 1;
            if (prop_cd <= 0)
            {
                prop_cd = Random.Range(2, 5);
                int random_val = Random.Range(0, 10);
                int prop_id = 0;
                if (random_val > 3 && random_val <= 6)
                {
                    prop_id = 1;
                }
                else if (random_val > 6)
                {
                    prop_id = 2;
                }
                GameObject prop_obj = Instantiate(props[prop_id], new Vector3(2 + (2 * i), dome_heights[i], 2), Quaternion.identity) as GameObject;
                prop_obj.transform.parent = prop_collection.transform;
            }
        }

        yield return new WaitForEndOfFrame();
    }
}