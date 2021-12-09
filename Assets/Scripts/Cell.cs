using Febucci.UI;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class Cell
{
    public bool IsMine { get; set; }
    private TextAnimator TextAnimator { get; set; }
    private TextMeshProUGUI DebugText { get; set; }
    public Button Button { get; private set; }
    private GameObject HoleSprite { get; set; }
    private GameObject MineSprite { get; set; }
    private GameObject ExplosionSprite { get; set; }
    public bool Visible { get; set; }
    public bool Locked { get { return !Button.interactable; } }
    public int MinesSurrounding { get; private set; }


    public Cell(TextAnimator textAnimator, TextMeshProUGUI debugText)
    {
        IsMine = false;
        TextAnimator = textAnimator;
        DebugText = debugText;
        Button = TextAnimator.GetComponentInParent<Button>();
        HoleSprite = Button.transform.Find("Hole Sprite").gameObject;
        MineSprite = Button.transform.Find("Mine Sprite").gameObject;
        ExplosionSprite = Button.transform.Find("Explosion").gameObject;
        Visible = false;
    }

    public Cell(bool isMine, TextAnimator textAnimator, TextMeshProUGUI debugText)
    {
        IsMine = isMine;
        TextAnimator = textAnimator;
        DebugText = debugText;
        Button = TextAnimator.GetComponentInParent<Button>();
        HoleSprite = Button.transform.Find("Hole Sprite").gameObject;
        MineSprite = Button.transform.Find("Mine Sprite").gameObject;
        ExplosionSprite = Button.transform.Find("Explosion").gameObject;
        Visible = false;
    }

    public void Uncover()
    {
        if (IsMine)
        {
            MineSprite.SetActive(true);
            ExplosionSprite.SetActive(false);
            ExplosionSprite.SetActive(true);
        }
        else
            HoleSprite.SetActive(true);
        Button.interactable = false;
        Visible = true;
    }

    public void Cover()
    {
        TextAnimator.SetText("", false);
        DebugText.SetText("");
        MineSprite.SetActive(false);
        HoleSprite.SetActive(false);
        ExplosionSprite.SetActive(false);
        Button.interactable = true;
        Visible = false;
    }

    public int CheckSurroundingMines(Vector2Int index, Cell[,] cells)
    {
        MinesSurrounding = 0;
        Vector2Int relInd;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 || y != 0)
                {
                    relInd = new Vector2Int(index.x + x, index.y + y);
                    if (relInd.x >= 0 && relInd.x < 9 && relInd.y >= 0 && relInd.y < 9)
                        if (cells[relInd.x, relInd.y].IsMine)
                            MinesSurrounding++;
                }
            }
        }
        TextAnimator.SetText(MinesSurrounding.ToString(), false);

        return MinesSurrounding;
    }

    public void SetDebugText(string text) => DebugText.SetText(text);
}
