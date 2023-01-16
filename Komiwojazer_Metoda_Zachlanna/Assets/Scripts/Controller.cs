using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Controller : MonoBehaviour
{
    //private bool go_to_start = true;
    private LineRenderer line;
    private int number_of_points = 4;
    private Vector3 starting_point;
    private Vector3 ending_point;
    private GameObject car;
    private Quaternion _lookRotation;
    private Vector3 _direction;
    public GameObject point;
    private float min_distance = 0;
    private int number_of_point = 0;
    private int point_count = 1;
    private bool is_riding = false;
    private bool started = false;
    private bool refresh_objects = false;
    private bool draw_new_line = false;
    private bool draw_last = true;
    private bool back_to_start = false;
    private bool draw_start = true;
    private double total_distance = 0;
    private Text tot_distance_text;
    GameObject[] objects;
    void Start()
    {
        objects = GameObject.FindGameObjectsWithTag("punkt");
        starting_point = GameObject.Find("Car").transform.position;
        ending_point = GameObject.Find("Finish_point").transform.position;
        car = GameObject.Find("Car");
        line = GameObject.Find("Line").GetComponent<LineRenderer>();
        line.gameObject.SetActive(true);
        tot_distance_text = GameObject.Find("tot_distance").GetComponent<Text>();
    }
    private bool get_distance_to_next_point()                                                                   //pobieranie pozycji najblizszego aktualnie punktu
    {
        min_distance = Vector3.Distance(objects[0].transform.position, car.transform.position);
        number_of_point = 0;
        for (int i = 1; i < number_of_points; i++)
        {
            float distance = Vector3.Distance(objects[i].transform.position, car.transform.position);
            if (min_distance > distance)
            {
                min_distance = distance;
                number_of_point = i;
            }
        }
        total_distance += min_distance;
        tot_distance_text.text = "Distance: " + Mathf.Round((float)total_distance).ToString();
        _direction = (objects[number_of_point].transform.position - car.transform.position).normalized;
        _lookRotation = Quaternion.LookRotation(_direction);
        car.transform.rotation = Quaternion.RotateTowards(car.transform.rotation, _lookRotation, 360);
        Debug.Log(objects.Length);
        draw_new_line = true;
        return true;
    }
    void Update()
    {
        if (refresh_objects)            // restartujemy i generujemy na nowo wszystkie punkty
        {
            destroying_points();
            creating_points();
            refresh_objects = false;
            is_riding = false;
            started = true;
        }

        if (started)
        {
            if (!is_riding)
            {
                is_riding = get_distance_to_next_point();
            }
            
            else
            {
                if (number_of_points > 0)
                {
                   car.transform.position = Vector3.MoveTowards(car.transform.position, objects[number_of_point].transform.position, 10 * Time.deltaTime);
                    if (draw_new_line)
                    {
                        line.positionCount++;
                        line.SetPosition(point_count, objects[number_of_point].transform.position);
                        point_count++;
                        draw_new_line = false;
                    }
                    if (car.transform.position == objects[number_of_point].transform.position)
                    {
                        Destroy(objects[number_of_point].gameObject);
                        for (int i = number_of_point; i < number_of_points - 1; i++)
                        {
                            objects[i] = objects[i + 1];
                        }
                        number_of_points--;
                        if(number_of_points > 0)
                        {
                            is_riding = false;
                        }
                        Debug.Log(number_of_points);
                    }
                }
                else
                {
                    if (!back_to_start)
                    {
                        _direction = (ending_point - car.transform.position).normalized;
                        _lookRotation = Quaternion.LookRotation(_direction);
                        car.transform.rotation = Quaternion.RotateTowards(car.transform.rotation, _lookRotation, 360);
                        car.transform.position = Vector3.MoveTowards(car.transform.position, ending_point, 10 * Time.deltaTime);
                        if (draw_last)
                        {
                            line.positionCount++;
                            line.SetPosition(point_count, ending_point);
                            point_count++;
                            draw_last = false;
                            total_distance += Vector3.Distance(ending_point, car.transform.position);
                            tot_distance_text.text = "Distance: " + Mathf.Round((float)total_distance).ToString();
                        }
                        if (car.transform.position == ending_point)
                        {
                            back_to_start = true;

                        }
                    }
                    else
                    {
                        _direction = (starting_point - car.transform.position).normalized;
                        _lookRotation = Quaternion.LookRotation(_direction);
                        car.transform.rotation = Quaternion.RotateTowards(car.transform.rotation, _lookRotation, 360);
                        car.transform.position = Vector3.MoveTowards(car.transform.position, starting_point, 10 * Time.deltaTime);
                        if (draw_start)
                        {
                            tot_distance_text.enabled = true;
                            line.positionCount++;
                            line.SetPosition(point_count, starting_point);
                            total_distance += Vector3.Distance(starting_point, car.transform.position);
                            tot_distance_text.text = "Distance: " + Mathf.Round((float)total_distance).ToString();
                            Debug.Log(total_distance);
                            draw_start = false;
                        }
                    }
                }
                
            }
            
        }
    }
    private void destroying_points()                                                                                             //usuwanie punktow
    {
        for (var i = 0; i < objects.Length; i++)
        {
            DestroyImmediate(objects[i]);
        }
    }
    private void creating_points()                                                                                              //tworzenie punktow
    {
        if (int.TryParse(GameObject.Find("number_of_points_text").GetComponent<Text>().text.ToString(), out int result))
        {
            number_of_points = result;
        }
        else
        {
            number_of_points = 4;                   //defaultowa liczba
        }
        for (int i = 0; i < number_of_points; i++)
        {
            GameObject x = Instantiate(point);
            x.transform.position = new Vector3(Random.Range(-35, 35), 0, Random.Range(2, 38));
            x.tag = "punkt";                                
        }
        objects = GameObject.FindGameObjectsWithTag("punkt");  
    }
    private void set_default()
    {
        car.transform.position = starting_point;        // przywracamy samochodzik do pozycji startowej
    }
    public void generate_points()       // funkcja wywolywania po kliknieciu przycisku start
    {
        back_to_start = false;
        draw_start = true;
        total_distance = 0;
        point_count = 1;
        line.SetVertexCount(1);
        draw_last = true;
        line.SetPosition(0, starting_point);
        started = false;
        set_default();
        refresh_objects = true; 
    }
}
