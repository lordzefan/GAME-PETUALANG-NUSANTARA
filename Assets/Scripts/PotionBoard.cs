using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    //size of the board
    public int width = 6;
    public int height = 8;
    public int reloadTime = 3;
    private bool isReloading = false;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference for our potion prefebs
    public GameObject[] potionPrefebs;
    //get a reference for the collection nodes potionboard + gameobject
    public Node[,] potionBoard;
    public List<GameObject> potionToDestroy = new ();
    public GameObject potionParent;
    [SerializeField]private Potion selectedPotion;
    [SerializeField]private bool isProcessingMove;
    [SerializeField]List<Potion> potionsToRemove = new();
    public TextMeshProUGUI reloadTxt;

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
        reloadTxt.text = reloadTime.ToString();
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
        spacingY = (float)(height - 1) / 2;

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
                potion.transform.SetParent(potionParent.transform);
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

    public void ReloadTime()
    {
        if (reloadTime > 0)
        {
            ReloadBoard();
        }
    }

    void ReloadBoard()
    {
        if (isReloading) return;
        isReloading = true;

        foreach (GameObject potion in potionToDestroy)
        {
            if (potion != null) Destroy(potion);
        }
        potionToDestroy.Clear();
        potionBoard = null;

        InitializedBoard();
        reloadTime--;
        reloadTxt.text = reloadTime.ToString();
        Debug.Log("Potion to destroy count: " + potionToDestroy.Count);

        isReloading = false;
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
        if(GameManager.instance.isGameEnded)
            return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        potionsToRemove.Clear();        

        foreach (Node nodePotion in potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }
        
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
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);

                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);
                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                                pot.isMatched = true;
                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMove)
    {
        foreach (Potion potionToRemove in potionsToRemove)
            {
                potionToRemove.isMatched = false;
            }
        RemoveAndRefill(potionsToRemove);
        GameManager.instance.ProcessTurn(potionsToRemove.Count, _subtractMove);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    #region  Cascading Potion

    private void RemoveAndRefill(List<Potion> _potionToRemove)
    {
        // remove the potion and clearing the board at location
        foreach (Potion potion in _potionToRemove)
        {
            //getting its x and y indicies and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            //destroy potion
            Destroy(potion.gameObject);

            //create a blank node on the potion board
            potionBoard[_xIndex,_yIndex] = new Node(true, null);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(potionBoard[x, y ].potion == null && potionBoard[x, y].isUsable == true)
                {
                    Debug.Log("the location X: "+ x + "Y: "+ y+ "is empty, attemp to refill it");
                    RefillPotion(x,y);
                }
            }
        }
    }

    private void RefillPotion(int x, int y)
    {
        // y offset
        int yOffsett = 1;

        // while the cell above our current cell is null and we're bellow the height of the board
        while (y + yOffsett < height && 
      (potionBoard[x, y + yOffsett].potion == null || !potionBoard[x, y + yOffsett].isUsable))
        {
            // increment y offset
            Debug.Log("the potion above is null, but iam not at the top of the board yet, so add yofset and try again");
            yOffsett++;
        }

        //we've either hit the top of the board or we found a potion
        if(y + yOffsett < height && potionBoard[x,y + yOffsett].potion != null)
        {
            // we've found a potion

            Potion potionAbove = potionBoard[x, y + yOffsett].potion.GetComponent<Potion>();

            //Move to correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            Debug.Log("i found the potion when i refill the board and it was in the location: ["+ x + ", " + (y + yOffsett)+ "] we have moved it to the location: ["+ x + ", " + y + "]");
            //move to location
            potionAbove.MoveToTarget(targetPos);
            // update incidices
            potionAbove.SetIndicies(x, y);
            //update our potion board
            potionBoard[x, y] = potionBoard[x, y + yOffsett];
            // set the location the potion came from null
            potionBoard[x, y + yOffsett] = new Node(true, null);            
        }

        //if we've hit the top of the board without finding a potion
        if (y + yOffsett == height)
        {
            Debug.Log("i've reached the top of the board without finding a potion");
            SpawnPotionAtTop(x);
        }
    }

   private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMove = height - index;
        Debug.Log("about to spawn a potion, ideally i'd like to put in the index of: "+ index);

        int randomIndex = Random.Range(0, potionPrefebs.Length);
        GameObject newPotion = Instantiate(potionPrefebs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        potionBoard[x, index] = new Node(true, newPotion);

        // ✅ Tambahkan ke daftar untuk dihancurkan nanti saat reload
        potionToDestroy.Add(newPotion);

        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMove, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);
    }


    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = -1;
        for (int y = height -1; y >= 0; y--)
        {
            if (potionBoard[x, y].potion == null && potionBoard[x, y].isUsable)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    


   
    #endregion

   private MatchResult SuperMatch(MatchResult _matchResult)
{
    List<Potion> superConnected = new List<Potion>(_matchResult.connectedPotions);
    HashSet<Potion> uniquePotions = new HashSet<Potion>(superConnected);

    foreach (Potion pot in _matchResult.connectedPotions)
    {
        List<Potion> extraConnectedHorizontal = new();
        List<Potion> extraConnectedVertical = new();

        // Cek horizontal tambahan
        CheckDirection(pot, new Vector2Int(1, 0), extraConnectedHorizontal);
        CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedHorizontal);

        // Cek vertical tambahan
        CheckDirection(pot, new Vector2Int(0, 1), extraConnectedVertical);
        CheckDirection(pot, new Vector2Int(0, -1), extraConnectedVertical);

        if (extraConnectedHorizontal.Count >= 2)
        {
            foreach (var p in extraConnectedHorizontal)
                uniquePotions.Add(p);
        }

        if (extraConnectedVertical.Count >= 2)
        {
            foreach (var p in extraConnectedVertical)
                uniquePotions.Add(p);
        }
    }

    if (uniquePotions.Count > _matchResult.connectedPotions.Count)
    {
        Debug.Log("Super match detected!");
        return new MatchResult
        {
            connectedPotions = new List<Potion>(uniquePotions),
            direction = MatchDirection.Super
        };
    }
    else
    {
        return _matchResult;
    }
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
                direction = MatchDirection.Horizontal
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
                direction = MatchDirection.Vertical
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
            if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
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
            _potion.SetSelected(true);
        }
        // if we select the same potion twice, then lets make selectedPotion null
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
            _potion.SetSelected(false);
        }
        // if selectedPotion is not null and is not CurrentUserAdminCheckResponse potion, attemp a swap
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion, _potion);
            selectedPotion.SetSelected(false);
            _potion.SetSelected(false);
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

        if (CheckBoard())
        {
            // start corutine that is going to process our matches in our turn.
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
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
