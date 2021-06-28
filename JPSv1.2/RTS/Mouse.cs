using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Mouse : MonoBehaviour
{
    public List<Unit> units = new List<Unit>();
    public JPS jps;
    Vector2 startPos;
    [SerializeField] GameObject selectionAreaObj;

    private void Start()
    {
        jps = GetComponent<JPS>();
        selectionAreaObj.SetActive(false);
    }
    void Update()
    {
        SelectionAreaBehaviour();

        if(Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            foreach (Unit u in units)
            {
               // Vector3[] newPath = jps.TestFindPath(u.transform.position, mousePos);

                //u.OnPath(newPath);
                 pathQuestManager.RequestPath(u.transform.position, mousePos, u.OnPathFound);
            }
        }
    }


    void SelectionAreaBehaviour()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionAreaObj.SetActive(true);
            startPos = GetMouseWorldPosition();
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 curMousePos = GetMouseWorldPosition();

            Vector2 bottomLeft = new Vector2(
                Mathf.Min(startPos.x, curMousePos.x),
                Mathf.Min(startPos.y, curMousePos.y));
            Vector2 topRight = new Vector2(
                Mathf.Max(startPos.x, curMousePos.x),
                Mathf.Max(startPos.y, curMousePos.y));


            selectionAreaObj.transform.position = bottomLeft;
            selectionAreaObj.transform.localScale = topRight - bottomLeft;
        }

        if (Input.GetMouseButtonUp(0))
        {

            selectionAreaObj.SetActive(false);
            Collider2D[] colArr = Physics2D.OverlapAreaAll(startPos, GetMouseWorldPosition());

            foreach (Unit u in units)
            {
               u.MouseSelectedVisible(false);
            }

            units.Clear();

            foreach (Collider2D col in colArr)
            {
                Unit unit = col.GetComponent<Unit>();

                if (unit != null)
                {
                     unit.MouseSelectedVisible(true);
                    units.Add(unit);
                }
                else
                {
                    return;
                }
            }
        }
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 cameraPos = Camera.main.ScreenToWorldPoint(mousePos);

        return cameraPos;
    }
}
