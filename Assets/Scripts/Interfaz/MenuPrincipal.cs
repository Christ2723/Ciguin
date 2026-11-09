using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Menú de inicio: Jugar, Cómo jugar e Historia. Enter también empieza la partida.
public class MenuPrincipal : MonoBehaviour {

	public GameObject panelPrincipal;
	public GameObject panelControles;
	public GameObject panelHistoria;
	public Text txtMejorPuntaje;
	public Text txtParpadeo;

	void Start() {
		Time.timeScale = 1f;
		Juego.I.Musica(Juego.I.musicaMenu);
		Mostrar(panelPrincipal);
		txtMejorPuntaje.text = "MEJOR PUNTAJE: " + Juego.MejorPuntaje.ToString("000000");
	}

	void Update() {
		txtParpadeo.enabled = (Time.time % 1.2f) < 0.8f;
		if (panelPrincipal.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) Jugar();
		if (!panelPrincipal.activeSelf && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))) Volver();
	}

	void Mostrar(GameObject panel) {
		panelPrincipal.SetActive(panel == panelPrincipal);
		panelControles.SetActive(panel == panelControles);
		panelHistoria.SetActive(panel == panelHistoria);
	}

	public void Jugar() {
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.ReiniciarPartida();
		SceneManager.LoadScene("Nivel1");
	}

	public void Controles() {
		Juego.I.Sonar(Juego.I.sBoton);
		Mostrar(panelControles);
	}

	public void Historia() {
		Juego.I.Sonar(Juego.I.sBoton);
		Mostrar(panelHistoria);
	}

	public void Volver() {
		Juego.I.Sonar(Juego.I.sBoton);
		Mostrar(panelPrincipal);
	}
}
