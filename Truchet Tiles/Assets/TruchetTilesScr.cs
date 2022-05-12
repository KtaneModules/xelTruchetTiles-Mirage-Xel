using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class TruchetTilesScr : MonoBehaviour {
    public KMBombModule Module;
    public KMAudio Sound;
    public KMSelectable SubmitButton;

    public KMSelectable[] PatternButtons;
    public TextMesh[] PatternArrows;
    public MeshRenderer[] TileRenderers;
    public Material[] TileMats;

    int[] _modulePattern = new int[4];
    int[] _userPattern = new int[] { 0, 0, 0, 0 };

    int _moduleId;
    static int _moduleIdCounter = 1;
    bool _solved;
	// Use this for initialization
	void Start () {
        _moduleId = _moduleIdCounter++;
        GeneratePuzzle();
        for (int i = 0; i < 4; i++)
        {           
            PatternButtons[i].OnInteract += PatternButtonPress(i);
        }
        SubmitButton.OnInteract += SubmitButtonPress();
	}
	
	// Update is called once per frame
	void GeneratePuzzle () {
        int tileIndex = Rnd.Range(0, 25);
        for (int i = 0; i < 4; i++)
            _modulePattern[i] = Rnd.Range(0, 4);
        for (int i = 0; i < 36; i++)
        {
            TileRenderers[i].material = TileMats[tileIndex];
            TileRenderers[i].transform.localEulerAngles = new Vector3(0, 90 * _modulePattern[i % 4], 0);
        }
        Debug.LogFormat("[Truchet Tiles #{0}] The pattern on the module is {1} in tileset {2}.", _moduleId, _modulePattern.Select(x => "URDL"[x]).Join(""), tileIndex + 1);
    }
    KMSelectable.OnInteractHandler PatternButtonPress(int index)
    {
        return delegate
        {
            if (!_solved)
            {
                PatternButtons[index].AddInteractionPunch(1f);
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                _userPattern[index] = (_userPattern[index] + 1) % 4;
                PatternArrows[index].text = "▲▶▼◀"[_userPattern[index]].ToString();
            }
            return false;
        };
    }
    KMSelectable.OnInteractHandler SubmitButtonPress()
    {
        return delegate
        {
            if (!_solved)
            {
                SubmitButton.AddInteractionPunch(1f);
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat("[Truchet Tiles #{0}] You submitted {1}.", _moduleId, _userPattern.Select(x => "URDL"[x]).Join(""));
                if (_userPattern.SequenceEqual(_modulePattern))
                {
                    Debug.LogFormat("[Truchet Tiles #{0}] That was correct. Module solved.", _moduleId);
                    Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Module.HandlePass();
                    _solved = true;
                }
                else
                {
                    Debug.LogFormat("[Truchet Tiles #{0}] That was incorrect. Strike!", _moduleId);
                    Module.HandleStrike();
                    _userPattern= new int[] { 0, 0, 0, 0 };
                    PatternArrows.ToList().ForEach(x => x.text = "▲");
                    GeneratePuzzle();
                }
            }
            return false;
        };
    }
    string TwitchHelpMessage = "Use '!{0} urdl' to submit the pattern.";
        IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.ToLowerInvariant();
        if (command.Contains(" "))
            yield break;
        yield return null;
        if (command.Length > 4) {
            yield return "sendtochaterror Too many parameters!";
            yield break;
        }
        if (command.Length < 4)
        {
            yield return "sendtochaterror Not enough parameters!";
            yield break;
        }
        foreach (char i in command)
        {
            if (!"urdl".Contains(i))
            {
                yield return string.Format("sendtochaterror Unrecognized parameter '{0}'!", i);
                yield break;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < "urdl".IndexOf(command[i]); j++)   
                {
                PatternButtons[i].OnInteract();
                yield return new WaitForSeconds(0.1f);
                }
        }
        SubmitButton.OnInteract();
        }

    
}
