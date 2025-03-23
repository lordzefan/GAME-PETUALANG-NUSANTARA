using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    //size of the board
    public int width = 6;
    public int height = 8;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference for our potion prefebs
    public GameObject[] potionPrefebs;
    //get a reference for the collection nodes potionboard + gameobject
    public Node[,] potionBoard;
    public List<GameObject> potionToDestroy = new ();
    [SerializeField]private Potion selectedPotion;
    [SerializeField]private bool isProcessingMove;

    //layout array
    public ArrayLayout arrayLayout;
    // public static of potionboard
    public static PotionBoard Instace;

    void Awake()
    {
        Instace = this;
    }
    void Start()
    {
        InitializedBoard();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if(isProcessingMove)
                return;

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("i have click a potion it is = "+ potion.gameObject);

                SelectPotion(potion);                
            }
        }
    }

    void InitializedBoard()
    {
        DestroyPotion();
        potionBoard = new Node [width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = ((float)(height - 1) / 2)+ 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node (false, null);
                }
                else
                {
                int randomIndex = Random.Range(0, potionPrefebs.Length);

                GameObject potion = Instantiate(potionPrefebs[randomIndex], position, Quaternion.identity);
                potion.GetComponent<Potion>().SetIndicies(x,y);
                potionBoard[x, y] = new Node(true,potion);
                potionToDestroy.Add(potion);
                }
            }
        }
        if(CheckBoard())
        {
            Debug.Log("have a matched lets star again");
            InitializedBoard();
        }else
        {
            Debug.Log("there no matched");
        }
    }

    private void DestroyPotion()
    {
        if (potionToDestroy != null)
        {
            foreach (GameObject potion in potionToDestroy)
            {
                Destroy(potion);
            }
            potionToDestroy.Clear();
        }
    }

    public bool CheckBoard()
    {
        Debug.Log("Checking Board");
        bool hasMatched = false;

        List<Potion> potionToRemove = new();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Checking if potion is usable
                if (potionBoard[x,y].isUsable)
                {
                    //then proceed to get class in node
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    //ensure its not matched
                    if (!potion.isMatched)
                    {
                        //run some matching logic
                        MatchResult matchedPotions  = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            //complex matching...

                            potionToRemove.AddRange(matchedPotions.connectedPotions);
                            foreach (Potion pot in matchedPotions.connectedPotions)
                                pot.isMatched = true;
                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new ();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        //check left
        CheckDirection(potion, new Vector2Int(1,0), connectedPotions);
        //check right
        CheckDirection(potion, new Vector2Int(-1,0), connectedPotions);
        //have three match (horizontal match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("have normal horizotal match" + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }
        //have for more than 3 (long horizontal match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("have long horizotal match" + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }
        //Clear out the connected potions
        connectedPotions.Clear();
        //read our initial potion
        connectedPotions.Add(potion);
        //check up
        CheckDirection(potion, new Vector2Int(0,1), connectedPotions);
        //check down
        CheckDirection(potion, new Vector2Int(0,-1), connectedPotions);
        //have three match (vertical match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("have normal Vertical match" + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        //have for more than 3 (long vertical match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("have long vertical match" + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction =MatchDirection.None
            };
        }
    }


    //check direction
    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we are within the bounfaries of the board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x,y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x,y].potion.GetComponent<Potion>();

                //does our potion match ? it also not be match
                if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                } 
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }

    #region Swapping Potion

    //select potion
    public void SelectPotion(Potion _potion)
    {
        // if we dont have a potion currently selected, then set the potion i just click to my selectedpotion
        if (selectedPotion == null)
        {
            Debug.Log(_potion);
            selectedPotion = _potion;
        }
        // if we select the same potion twice, then lets make selectedPotion null
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
        }
        // if selectedPotion is not null and is not CurrentUserAdminCheckResponse potion, attemp a swap
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion, _potion);
            selectedPotion = null;
        }
        // selectedPotion back to null
    }

    //swap potion - logic
    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        // is adjacent dont do anything
        if (!IsAdjacent(_currentPotion , _targetPotion))
        {
            return;
        }
        DoSwap(_currentPotion, _targetPotion);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentPotion , _targetPotion));
    }
    //do swap
    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;
        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        // update indicies.
        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }
    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);
        bool hasMatch = CheckBoard();

        if (!hasMatch)
        {
            DoSwap(_currentPotion, _targetPotion);
        }
        isProcessingMove = false;
    }

    // is adjacent
    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex -_targetPotion.yIndex) == 1;
    }
    //process matches

    #endregion
}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
