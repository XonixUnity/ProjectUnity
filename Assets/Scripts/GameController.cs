using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameObject cube_prefab;
    public GameObject player_prefab;
    public GameObject inner_bot_prefab;

    public Transform cubes_transform;

    public Text hp_text;

    public float speed;
    public int hp;

    GameObject spawned_player;

    const float max_x = 7.4f;
    const float max_y = 4.4f;

    int percent;
    int temp_percent;
    int level = 1;

    Coroutine player_move = null;
    List<Coroutine> inner_bot_move = new List<Coroutine>();

    Color blue;

    List<Renderer> pos_to_del = new List<Renderer>();
    List<GameObject> inner_bots = new List<GameObject>();
    
    private void Start()
    {
        blue = new Color(0, 0.5f, 1);

        hp_text.text = "HP: " + hp.ToString();

        for (float x = -max_x; x <= max_x; x += 0.2f)
        {
            for (float y = -max_y; y <= max_y; y += 0.2f)
            {
                Vector3 pos = new Vector3((float)System.Math.Round(x, 2), (float)System.Math.Round(y, 2), 2);
                GameObject spawned_cube = Instantiate(cube_prefab, pos, Quaternion.identity, cubes_transform);
                spawned_cube.GetComponent<Renderer>().material.color = blue;
                
                if (pos.x < -max_x + 0.4f || pos.x > max_x - 0.4f)
                {
                    spawned_cube.GetComponent<Renderer>().material.color = Color.black;
                    percent--;
                }
                if (pos.y < -max_y + 0.4f || pos.y > max_y - 0.4f)
                {
                    spawned_cube.GetComponent<Renderer>().material.color = Color.black;
                    percent--;
                }
                percent++;
            }
        }

        int multiplier = Random.Range(0, 21);

        int multiplier_1 = Random.Range(-1, 2);

        while (multiplier_1 == 0)
        {
            multiplier_1 = Random.Range(-1, 2);
        }

        Vector3 p = new Vector3(multiplier * multiplier_1 * 0.2f, multiplier * multiplier_1 * 0.2f, 0);

        inner_bots.Add(Instantiate(inner_bot_prefab, p, Quaternion.identity));

        for (int i = 0; i < inner_bots.Count; i++)
        {
            inner_bots[i].GetComponent<Renderer>().material.color = Color.white;

            inner_bot_move.Add(StartCoroutine(Move_Inner_bot(move_direction().x * 0.2f, move_direction().y * 0.2f, true, i)));
        }

        spawned_player = Instantiate(player_prefab, new Vector3(-max_x, max_y, 0), Quaternion.identity);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { if (player_move != null) StopCoroutine(player_move); player_move = StartCoroutine(Move_player(0, 0.2f, true)); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { if (player_move != null) StopCoroutine(player_move); player_move = StartCoroutine(Move_player(0, -0.2f, true)); }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { if (player_move != null) StopCoroutine(player_move); player_move = StartCoroutine(Move_player(-0.2f, 0, true)); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { if (player_move != null) StopCoroutine(player_move); player_move = StartCoroutine(Move_player(0.2f, 0, true)); }
    }

    Vector2 move_direction()
    {

        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);

        while (x == 0)
        {
            x = Random.Range(-1, 2);
        }

        while (y == 0)
        {
            y = Random.Range(-1, 2);
        }

        return new Vector2(x, y);
    }

    void CheckCubeColor(float x, float y)
    {
        Vector3 p = spawned_player.transform.position;
        Vector3 pos = new Vector3(p.x - x, p.y - y, 1);

        RaycastHit hit;
        RaycastHit hit1;

        if (Physics.Raycast(p, Vector3.forward, out hit))
        {
            if (hit.transform.GetComponent<Renderer>().material.color == Color.magenta)
            {
                StopCoroutine(player_move);
                for (int i = 0; i < inner_bot_move.Count; i++)
                {
                    StopCoroutine(inner_bot_move[i]);
                }
                pos_to_del.Clear();

                hp--;
                hp_text.text = "HP: " + hp.ToString();
                StartCoroutine(Lose());
            }
        }

        if (Physics.Raycast(p, Vector3.forward,out hit))
        {
            if (hit.transform.GetComponent<Renderer>().material.color == blue)
            {
                hit.transform.GetComponent<Renderer>().material.color = Color.magenta;
                pos_to_del.Add(hit.transform.GetComponent<Renderer>());
            }
        }

        if (Physics.Raycast(p, Vector3.forward, out hit) && Physics.Raycast(pos, Vector3.forward, out hit1))
        {
            if (hit.transform.GetComponent<Renderer>().material.color == Color.black
                && hit1.transform.GetComponent<Renderer>().material.color == Color.magenta)
            {
                StopCoroutine(player_move);

                for (int i = 0; i < pos_to_del.Count; i++)
                {
                    pos_to_del[i].material.color = Color.black;
                    temp_percent++;
                }
                pos_to_del.Clear();

                foreach (var inner in inner_bots)
                {
                    CheckDelete(inner.transform.position.x, inner.transform.position.y);
                }
                for (float yi = -max_y; yi < max_y; yi += 0.2f)
                {
                    for (float xi = -max_x; xi < max_x; xi += 0.2f)
                    {
                        if (Physics.Raycast(new Vector3(xi, yi, 1), Vector3.forward, out hit))
                        {
                            if (hit.transform.GetComponent<Renderer>().material.color == blue)
                            {
                                hit.transform.GetComponent<Renderer>().material.color = Color.black;
                                temp_percent++;
                            }
                            if (hit.transform.GetComponent<Renderer>().material.color == Color.green)
                            {
                                hit.transform.GetComponent<Renderer>().material.color = blue;
                            }
                        }
                    }
                }

            }
        }

        if ((float)temp_percent / (float)percent >= 0.7f)
        {
            Win();
        }

    }

    void CheckDelete(float x, float y)
    {
        Vector3 p = new Vector3(x, y, 1);

        RaycastHit hit;

        if (Physics.Raycast(p, Vector3.forward, out hit))
        {
            if (hit.transform.GetComponent<Renderer>().material.color == Color.black
                || hit.transform.GetComponent<Renderer>().material.color == Color.green) return;

            hit.transform.GetComponent<Renderer>().material.color = Color.green;

            for (int dx = -1; dx < 2; dx++)
            {
                for (int dy = -1; dy < 2; dy++)
                {
                    CheckDelete(x + dx * 0.2f, y + dy * 0.2f);
                }
            }
        }
    }
    
    IEnumerator Move_player(float x, float y, bool can_move)
    {
        while (can_move)
        {
            Vector3 p = spawned_player.transform.position;
            Vector3 pos = new Vector3(p.x + x, p.y + y, 1);

            if (Physics.Raycast(pos, Vector3.forward))
            {
                CheckCubeColor(x, y);
                spawned_player.transform.position += new Vector3(x, y, 0);
            }
            else { StopCoroutine(player_move); }

            yield return new WaitForSeconds((float)1 / speed);
        }
    }

    IEnumerator Move_Inner_bot(float x, float y, bool can_move, int index)
    {
        float temp_x = x;
        float temp_y = y;

        while (can_move)
        {
            Vector3 p = inner_bots[index].transform.position;
            Vector3 pos = new Vector3(p.x + x, p.y + y, 1);

            RaycastHit hit;

            if (Physics.Raycast(pos, Vector3.forward, out hit))
            {
                if (hit.transform.GetComponent<Renderer>().material.color == Color.black)
                {
                    Vector3 pos1 = new Vector3(p.x + x, p.y, 1);
                    if (Physics.Raycast(pos1, Vector3.forward, out hit))
                    {
                        if (hit.transform.GetComponent<Renderer>().material.color == Color.black)
                        {
                            temp_x = -x;
                        }
                    }

                    Vector3 pos2 = new Vector3(p.x, p.y + y, 1);
                    if (Physics.Raycast(pos2, Vector3.forward, out hit))
                    {
                        if (hit.transform.GetComponent<Renderer>().material.color == Color.black)
                        {
                            temp_y = -y;
                        }
                    }

                    if (temp_x == x && temp_y == y)
                    {
                        temp_x = -x;
                        temp_y = -y;
                    }

                    inner_bot_move.Add(StartCoroutine(Move_Inner_bot(temp_x, temp_y, true, index)));
                    break;
                }

                if (hit.transform.GetComponent<Renderer>().material.color == Color.magenta)
                {
                    StopAllCoroutines();

                    pos_to_del.Clear();
                    hp--;
                    hp_text.text = "HP: " + hp.ToString();
                    StartCoroutine(Lose());
                }

                inner_bots[index].transform.position += new Vector3(x, y, 0);
            }
                yield return new WaitForSeconds((float)1 / speed);
        }

        //inner_bot_move.Clear();

    }

    IEnumerator Lose()
    {
        yield return new WaitForSeconds(0.1f);

        if (hp <= 0)
        {
            SceneManager.LoadScene(0);
        }


        RaycastHit hit;

        for (float x = -max_x; x < max_x; x += 0.2f)
        {
            for (float y = -max_y; y < max_y; y += 0.2f)
            {
                if (Physics.Raycast(new Vector3(x, y, 1), Vector3.forward, out hit))
                {
                    if (hit.transform.GetComponent<Renderer>().material.color == Color.magenta)
                    {
                        hit.transform.GetComponent<Renderer>().material.color = blue;
                    }
                }
            }
        }

        spawned_player.transform.position = new Vector3(-max_x, max_y, 0);

        for (int i = 0; i < inner_bots.Count; i++)
        {
            inner_bot_move.Add(StartCoroutine(Move_Inner_bot(move_direction().x * 0.2f, move_direction().y * 0.2f, true, i)));
        }
    }

    void Win()
    {
        hp++;

        hp_text.text = "HP: " + hp;

        level++;

        temp_percent = 0;

        StopAllCoroutines();

        for (float x = -max_x; x <= max_x; x += 0.2f)
        {
            for (float y = -max_y; y <= max_y; y += 0.2f)
            {
                RaycastHit hit;
                Vector3 pos = new Vector3(x, y, 1);
                if (Physics.Raycast(pos, Vector3.forward, out hit))
                {
                    hit.transform.GetComponent<Renderer>().material.color = blue;

                    if (pos.x < -max_x + 0.4f || pos.x > max_x - 0.4f)
                    {
                        hit.transform.GetComponent<Renderer>().material.color = Color.black;
                    }
                    if (pos.y < -max_y + 0.4f || pos.y > max_y - 0.4f)
                    {
                        hit.transform.GetComponent<Renderer>().material.color = Color.black;
                    }
                }
            }
        }

        spawned_player.transform.position = new Vector3(-max_x, max_y, 0);

        int multiplier = Random.Range(0, 21);

        int multiplier_1 = Random.Range(-1, 2);

        while (multiplier_1 == 0)
        {
            multiplier_1 = Random.Range(-1, 2);
        }

        Vector3 p = new Vector3(multiplier * multiplier_1 * 0.2f, multiplier * multiplier_1 * 0.2f, 0);

        inner_bots.Add(Instantiate(inner_bot_prefab, p, Quaternion.identity));

        for (int i = 0; i < inner_bots.Count; i++)
        {
            inner_bots[i].GetComponent<Renderer>().material.color = Color.white;
            inner_bot_move.Add(StartCoroutine(Move_Inner_bot(move_direction().x * 0.2f, move_direction().y * 0.2f, true, i)));
        }

    }
}