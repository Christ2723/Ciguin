using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

//Toma capturas de las escenas (para revisar el arte y para el documento PDF)
public static class Capturas {

	const string CARPETA = "Documentacion/capturas";

	[MenuItem("Ciguin/Tomar capturas")]
	public static void TomarCapturas() {
		Directory.CreateDirectory(CARPETA);
		Escena("Menu", new[] { 13.34f }, "menu", true);
		Escena("Nivel1", new[] { 13.34f, 50f, 95f, 140f, 200f }, "nivel1", false);
		Escena("Nivel2", new[] { 13.34f, 60f, 100f, 135f, 190f }, "nivel2", false);
		Escena("Nivel3", new[] { 13.34f, 60f, 100f, 136f }, "nivel3", false);
		Escena("Victoria", new[] { 13.34f }, "victoria", true);
		Debug.Log("Capturas: listas");
	}

	static void Escena(string escena, float[] posiciones, string nombre, bool conUI) {
		EditorSceneManager.OpenScene("Assets/Scenes/" + escena + ".unity", OpenSceneMode.Single);
		var cam = Camera.main;
		var rt = new RenderTexture(1280, 720, 24);

		foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) {
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = cam;
			canvas.planeDistance = 1f;
			if (!conUI) {
				foreach (var t in canvas.GetComponentsInChildren<Transform>(true)) {
					if (t.name == "PanelPausa" || t.name == "PanelJefe" || t.name == "PanelMensaje" || t.name == "PanelControles" || t.name == "PanelHistoria") t.gameObject.SetActive(false);
				}
			} else {
				foreach (var t in canvas.GetComponentsInChildren<Transform>(true)) {
					if (t.name == "PanelControles" || t.name == "PanelHistoria") t.gameObject.SetActive(false);
				}
			}
		}
		var nivel = Object.FindAnyObjectByType<Nivel>();
		if (nivel != null) nivel.txtNivel.text = "NIVEL " + nivel.numero + "\n" + nivel.nombre.ToUpper();

		for (int i = 0; i < posiciones.Length; i++) {
			cam.transform.position = new Vector3(posiciones[i], 7.5f, -10f);
			Canvas.ForceUpdateCanvases();
			cam.targetTexture = rt;
			cam.Render();
			RenderTexture.active = rt;
			var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
			tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
			tex.Apply();
			File.WriteAllBytes(CARPETA + "/" + nombre + "_" + i + ".png", tex.EncodeToPNG());
			Object.DestroyImmediate(tex);
			cam.targetTexture = null;
			RenderTexture.active = null;
		}

		//Paneles extra del menú
		if (escena == "Menu") {
			foreach (var panelNombre in new[] { "PanelControles", "PanelHistoria" }) {
				foreach (var t in Object.FindAnyObjectByType<Canvas>().GetComponentsInChildren<Transform>(true)) {
					if (t.name == "PanelPrincipal") t.gameObject.SetActive(false);
					if (t.name == "PanelControles" || t.name == "PanelHistoria") t.gameObject.SetActive(t.name == panelNombre);
				}
				Canvas.ForceUpdateCanvases();
				cam.targetTexture = rt;
				cam.Render();
				RenderTexture.active = rt;
				var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
				tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
				File.WriteAllBytes(CARPETA + "/menu_" + panelNombre.Replace("Panel", "").ToLower() + ".png", tex.EncodeToPNG());
				Object.DestroyImmediate(tex);
				cam.targetTexture = null;
				RenderTexture.active = null;
			}
		}
		rt.Release();
	}
}
