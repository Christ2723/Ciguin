using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Pantalla final después de derrotar al Galipote
public class PantallaVictoria : MonoBehaviour {

	public Text txtPuntos;
	public Text txtParpadeo;

	void Start() {
		Time.timeScale = 1f;
		var j = Juego.I;
		bool record = j.puntos > Juego.MejorPuntaje;
		j.GuardarMejorPuntaje();
		j.Musica(j.musicaVictoria);
		txtPuntos.text = "PUNTOS: " + j.puntos.ToString("000000") + "\nVIDAS RESTANTES: " + j.vidas +
			(record ? "\n¡NUEVO RÉCORD!" : "\nMEJOR PUNTAJE: " + Juego.MejorPuntaje.ToString("000000"));
	}

	void Update() {
		txtParpadeo.enabled = (Time.time % 1.2f) < 0.8f;
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Volver();
	}

	public void Volver() {
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.ReiniciarPartida();
		SceneManager.LoadScene("Menu");
	}
}
