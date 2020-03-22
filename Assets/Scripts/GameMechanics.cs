using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMechanics : MonoBehaviour
{
    GameData GD;
    Text text_info, text_dice, text_tiles, text_roll, text_winner;
    Button button_dice;

    public GameObject ui_dice_text, ui_info_text, ui_dice_button, ui_tiles_left, ui_rolls_so_far, ui_winner, ui_game, ui_end;

    public int player_cur, player_max;

    public bool can_roll, started;

    int dice_val = 0;

    float anim_time = 0.5f, dice_time = 0.01f;

    void Start()
    {
        GD = gameObject.GetComponent<GameData>();

        text_info = ui_info_text.GetComponent<Text>();
        text_dice = ui_dice_text.GetComponent<Text>();
        text_tiles = ui_tiles_left.GetComponent<Text>();
        text_roll = ui_rolls_so_far.GetComponent<Text>();
        text_winner = ui_winner.GetComponent<Text>();

        button_dice = ui_dice_button.GetComponent<Button>();

        can_roll = true;
    }

    public void StartGame ()
    {
        text_info.text = "It's " + GD.player_names[player_cur] + "'s turn! \nClick the dice to roll";
        text_roll.text = GD.player_rolls[player_cur] + " roll(s)";
        text_tiles.text = GD.player_tiles_left[player_cur] + " tile(s) left";
        started = true;
    }

    public void RollDice ()
    {
        if (can_roll)
            StartCoroutine(DiceMechanics());
    }

    public void NewGame ()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame ()
    {
        Application.Quit();
    }

    IEnumerator DiceMechanics ()
    {
        button_dice.interactable = false;
        can_roll = false;

        text_info.text = "Rolling the dice...";

        dice_val = 0;
        float wait_time = 0f;
        for (int i = 0; i < 30; i++)
        {
            wait_time += dice_time;
            dice_val = Random.Range(1, 7);
            text_dice.text = "" + dice_val;
            yield return new WaitForSeconds(wait_time);
        }

        text_info.text = GD.player_names[player_cur] + " can move " + dice_val + " step(s)!";
        GD.player_rolls[player_cur] += 1;
        text_roll.text = GD.player_rolls[player_cur] + " roll(s)";
        StartCoroutine(MovementMechanics());
        yield return new WaitForEndOfFrame();
    }

    IEnumerator MovementMechanics ()
    {
        int cur_tile = GD.player_position[player_cur];
        for (int i = 0; i < dice_val; i++)
        {
            cur_tile++;

            text_info.text = GD.player_names[player_cur] + " can move " + (dice_val - (i + 1)) + " step(s)!";
            StartCoroutine(LerpToPosition(anim_time, GD.domes[cur_tile].transform.position));
            GD.player_tiles_left[player_cur] -= 1;

            // We got a winner!
            if (GD.player_tiles_left[player_cur] == 0)
            {
                ui_game.SetActive(false);
                ui_end.SetActive(true);
                text_winner.text = GD.player_names[player_cur] + " has won the game!";
                yield break;
            }

            text_tiles.text = GD.player_tiles_left[player_cur] + " tile(s) left";
            yield return new WaitForSeconds(anim_time);
        }
        
        // compare current dome in 3 lists to see if there's a match for special cases
        if (GD.domes_good.Contains(GD.domes[cur_tile]))
        {
            // Randomize per dome per game, but the same for all players
            // move forward extra tiles; 3 for now
            for (int i = 0; i < 3; i++)
            {
                cur_tile++;

                text_info.text = GD.player_names[player_cur] + " can move " + (3 - i) + " extra step(s)!";
                StartCoroutine(LerpToPosition(anim_time, GD.domes[cur_tile].transform.position));
                GD.player_tiles_left[player_cur] -= 1;

                // We got a winner!
                if (GD.player_tiles_left[player_cur] == 0)
                {
                    ui_game.SetActive(false);
                    ui_end.SetActive(true);
                    text_winner.text = GD.player_names[player_cur] + " has won the game!";
                    yield break;
                }

                text_tiles.text = GD.player_tiles_left[player_cur] + " tile(s) left";
                yield return new WaitForSeconds(anim_time);
            }
        }
        else if (GD.domes_bad.Contains(GD.domes[cur_tile]))
        {
            // Randomize per dome per game, but the same for all players
            // move back few tiles; 2 for now
            for (int i = 0; i < 2; i++)
            {
                cur_tile--;

                text_info.text = GD.player_names[player_cur] + " moves back " + (2 - i) + " step(s)...";
                StartCoroutine(LerpToPosition(anim_time, GD.domes[cur_tile].transform.position));
                GD.player_tiles_left[player_cur] += 1;

                yield return new WaitForSeconds(anim_time);
            }
        }
        else if (GD.domes_special.Contains(GD.domes[cur_tile]))
        {
            // pick random minigame the player can play
            // sample minigame: reach a total of 20 eyes to get 5 free rolls in 1 turn! You can't move during this minigame
            // rules sample minigame: can only roll 1 time per turn, each roll stacks upon the previous one
            // once 20 has been reached the minigame will be completed and the player can move again
        }

        GD.player_position[player_cur] = cur_tile;

        button_dice.interactable = true;
        can_roll = true;
        player_cur += 1;
        if (player_cur == player_max) player_cur = 0;
        
        text_info.text = "It's " + GD.player_names[player_cur] + "'s turn! \nClick the dice to roll";
        text_roll.text = GD.player_rolls[player_cur] + " roll(s)";
        text_tiles.text = GD.player_tiles_left[player_cur] + " tile(s) left";
        yield return new WaitForEndOfFrame();
    }

    IEnumerator LerpToPosition(float time, Vector3 end_pos)
    {
        float elapsed_time = 0;
        Vector3 start_pos = GD.players[player_cur].transform.position;

        while (elapsed_time < time)
        {
            GD.players[player_cur].transform.position = Vector3.Lerp(start_pos, (end_pos + (Vector3.up / 2f)), (elapsed_time / time));
            elapsed_time += Time.deltaTime;
            yield return null;
        }

        GD.players[player_cur].transform.position = end_pos;
        yield return null;
    }
}