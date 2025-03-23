using System.Collections;
using System.Collections.Generic;
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

    void InitializedBoard()
    {
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
