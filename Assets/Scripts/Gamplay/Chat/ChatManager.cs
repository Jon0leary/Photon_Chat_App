using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;   // ScrollRect
using TMPro;

public class ChatManager : MonoBehaviourPun
{
    [Header("UI Refs")]
    [SerializeField] private GameObject   panelRoot;     // <-- WHOLE ChatPanel (ScrollView + input)
    [SerializeField] private GameObject   inputRoot;     // parent of the TMP_InputField (inside the panel)
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text       messagesText;
    [SerializeField] private ScrollRect     scrollRect;

    [Header("Limits")]
    [SerializeField] private int maxMessages    = 100;
    [SerializeField] private int maxCharsPerMsg = 200;

    public static bool IsTyping { get; private set; }

    void Awake()
    {
        // Start closed
        if (panelRoot) panelRoot.SetActive(false);
        if (inputRoot) inputRoot.SetActive(false);
        IsTyping = false;

        if (inputField)
        {
            inputField.lineType = TMP_InputField.LineType.MultiLineNewline; // Shift+Enter = newline
            inputField.onSubmit.AddListener(_ => TrySend());                 // Enter = send
        }
        if (messagesText) messagesText.richText = true;
    }

    void Update()
    {
        // A) Toggle WHOLE panel with T
        if (Input.GetKeyDown(KeyCode.T))
            TogglePanel();

        // If panel is closed, nothing else to do
        if (!panelRoot || !panelRoot.activeSelf || inputField == null) return;

        // C) Enter to send (unless Shift is held), Esc to close input/panel
        if (inputField.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Return) &&
               !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                TrySend();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
            }
        }
    }

    // ---- Panel control ----
    void TogglePanel()
    {
        if (!panelRoot) return;
        bool opening = !panelRoot.activeSelf;
        panelRoot.SetActive(opening);

        if (inputRoot) inputRoot.SetActive(opening);
        IsTyping = opening;

        if (opening && inputField)
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
            inputField.Select();
        }
        else if (!opening && inputField)
        {
            inputField.DeactivateInputField();
        }
    }

    void ClosePanel()
    {
        if (!panelRoot) return;
        panelRoot.SetActive(false);
        if (inputRoot) inputRoot.SetActive(false);
        IsTyping = false;
        if (inputField) inputField.DeactivateInputField();
    }

    // ---- Sending / Receiving ----
    void TrySend()
    {
        // NRE protection + limit text
        if (inputField == null) return;

        string msg = inputField.text;
        if (string.IsNullOrWhiteSpace(msg)) { inputField.text = ""; return; }
        if (msg.Length > maxCharsPerMsg) msg = msg.Substring(0, maxCharsPerMsg);

        string nick = PhotonNetwork.NickName ?? "Player";

        // Requires a PhotonView on this GameObject
        photonView.RPC(nameof(RpcReceive), RpcTarget.All, nick, msg);

        // clear & keep typing
        inputField.text = string.Empty;
        inputField.ActivateInputField();
        inputField.Select();
    }

    [PunRPC]
    void RpcReceive(string nick, string msg)
    {
        AppendLine($"<b>{Escape(nick)}:</b> {Escape(msg)}");
    }

    // ---- Helpers ----
    void AppendLine(string line)
    {
        if (!messagesText) return;

        // Trim backlog
        var txt = messagesText.text;
        int lines = 0;
        for (int i = 0; i < txt.Length; i++) if (txt[i] == '\n') lines++;
        if (lines > maxMessages)
        {
            int firstBreak = txt.IndexOf('\n');
            if (firstBreak >= 0) txt = txt[(firstBreak + 1)..];
        }

        messagesText.text = (txt.Length == 0 ? line : txt + "\n" + line);

        // Smart auto-scroll (only if already near bottom)
        if (scrollRect && IsAtBottom(scrollRect))
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    static bool IsAtBottom(ScrollRect sr) => sr.verticalNormalizedPosition <= 0.001f;

    static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
