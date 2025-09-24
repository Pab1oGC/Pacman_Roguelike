using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManagerHUD : MonoBehaviour
{
    public float uiScale = 1.6f; // Escala de toda la UI
    public int fontSize = 18;    // Tamaño de letra
    public int spacing = 12;     // Separación vertical entre botones

    NetworkManager manager;

    void Awake() => manager = GetComponent<NetworkManager>();

    void OnGUI()
    {
        // Escalar toda la IMGUI
        var prevMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1));

        // Guardar tipografía previa
        int prevBtn = GUI.skin.button.fontSize;
        int prevLbl = GUI.skin.label.fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.label.fontSize = fontSize;

        float w = 340f;
        GUILayout.BeginArea(new Rect(10, 10, w, Screen.height));

        // --- Igual que el HUD estándar, pero más grande y con espacio ---
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Start Host")) manager.StartHost();
                GUILayout.Space(spacing);
            }

            GUILayout.BeginHorizontal();
            manager.networkAddress = GUILayout.TextField(manager.networkAddress);
            if (GUILayout.Button("Start Client")) manager.StartClient();
            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);

            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Start Server")) manager.StartServer();
                GUILayout.Space(spacing);
            }
        }
        else
        {
            if (NetworkServer.active && NetworkClient.isConnected)
                GUILayout.Label($"Host: running ");

            if (NetworkServer.active && !NetworkClient.isConnected)
                GUILayout.Label($"Host: running ");

            if (NetworkClient.isConnected && !NetworkServer.active)
                GUILayout.Label($"Client: connected to {manager.networkAddress}");

            GUILayout.Space(spacing);

            if (GUILayout.Button(NetworkServer.active ? "Stop Host" : "Stop Client"))
            {
                if (NetworkServer.active && NetworkClient.isConnected) manager.StopHost();
                else if (NetworkClient.isConnected) manager.StopClient();
                else if (NetworkServer.active) manager.StopServer();
            }
        }

        GUILayout.EndArea();

        // Restaurar estilos
        GUI.skin.button.fontSize = prevBtn;
        GUI.skin.label.fontSize = prevLbl;
        GUI.matrix = prevMatrix;
    }
}
