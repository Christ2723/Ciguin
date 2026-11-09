using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Controla un nivel: tiempo límite, HUD, puntos de control, muerte del personaje,
//nivel completado, pausa y la pelea contra el jefe.
public class Nivel : MonoBehaviour {

	public static Nivel I;

	[Header("Datos del nivel")]
	public int numero = 1;
	public string nombre = "Playa de Samaná";
	public float tiempoLimite = 200f;
	public string escenaSiguiente = "Nivel2";
	public AudioClip musica;
	public AudioClip musicaJefe;
	public float anchoNivel = 200f;

	[Header("HUD")]
	public Text txtVidas;
	public Text txtLarimares;
	public Text txtPuntos;
	public Text txtTiempo;
	public Text txtNivel;
	public Image[] corazones;
	public GameObject panelMensaje;
	public Text txtMensaje;
	public Text txtSubMensaje;
	public GameObject panelPausa;
	public GameObject panelJefe;
	public Image barraJefe;
	public Font fuente;

	public float Tiempo { get; private set; }
	public bool Terminado { get; private set; }
	public bool Pausado { get; private set; }

	Vector3 puntoControl;
	Coroutine mensajeTemporal;

	void Awake() {
		I = this;
		Time.timeScale = 1f;
	}

	void Start() {
		Tiempo = tiempoLimite;
		puntoControl = Jugador.I.transform.position;
		Juego.I.Musica(musica);
		panelPausa.SetActive(false);
		panelJefe.SetActive(false);
		MostrarMensaje("NIVEL " + numero, nombre.ToUpper(), 2.5f);
		ActualizarHUD();
	}

	void Update() {

		if (!Terminado && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))) {
			Pausar(!Pausado);
		}
		if (Pausado) {
			if (Input.GetKeyDown(KeyCode.M)) {
				Time.timeScale = 1f;
				Juego.I.ReiniciarPartida();
				SceneManager.LoadScene("Menu");
			}
			return;
		}

		if (!Terminado) {
			Tiempo -= Time.deltaTime;
			if (Tiempo <= 0f) {
				Tiempo = 0f;
				Muerte("¡Se acabó el tiempo y no completaste el nivel!");
			}
		}
		ActualizarHUD();
	}

	void ActualizarHUD() {
		var j = Juego.I;
		txtVidas.text = "x" + j.vidas;
		for (int i = 0; i < corazones.Length; i++) corazones[i].enabled = i < j.vidas;
		txtLarimares.text = "x" + j.larimares.ToString("00");
		txtPuntos.text = "PUNTOS\n" + j.puntos.ToString("000000");
		int t = Mathf.CeilToInt(Tiempo);
		txtTiempo.text = "TIEMPO\n" + t.ToString("000");
		txtTiempo.color = t <= 30 && (Time.time % 0.6f) < 0.3f ? new Color(1f, 0.35f, 0.3f) : Color.white;
		txtNivel.text = "NIVEL " + numero + "\n" + nombre.ToUpper();
	}

	public void Pausar(bool pausar) {
		Pausado = pausar;
		Time.timeScale = pausar ? 0f : 1f;
		panelPausa.SetActive(pausar);
		Juego.I.PausarMusica(pausar);
		Juego.I.Sonar(Juego.I.sPausa);
	}

	// ------------------------------------------------ Mensajes

	public void MostrarMensaje(string titulo, string subtitulo, float segundos) {
		if (mensajeTemporal != null) StopCoroutine(mensajeTemporal);
		mensajeTemporal = StartCoroutine(MensajeTemporal(titulo, subtitulo, segundos));
	}

	IEnumerator MensajeTemporal(string titulo, string subtitulo, float segundos) {
		panelMensaje.SetActive(true);
		txtMensaje.text = titulo;
		txtSubMensaje.text = subtitulo;
		yield return new WaitForSeconds(segundos);
		if (!Terminado) panelMensaje.SetActive(false);
	}

	void MensajeFijo(string titulo, string subtitulo, Color color) {
		if (mensajeTemporal != null) StopCoroutine(mensajeTemporal);
		panelMensaje.SetActive(true);
		txtMensaje.text = titulo;
		txtMensaje.color = color;
		txtSubMensaje.text = subtitulo;
	}

	//Números que suben cuando se ganan puntos
	public void PuntosFlotantes(int cantidad, Vector3 posicion) {
		Juego.I.SumarPuntos(cantidad);
		var go = new GameObject("Puntos");
		go.transform.position = posicion + Vector3.up * 0.6f;
		var tm = go.AddComponent<TextMesh>();
		tm.font = fuente;
		tm.text = cantidad.ToString();
		tm.fontSize = 48;
		tm.characterSize = 0.08f;
		tm.anchor = TextAnchor.MiddleCenter;
		tm.fontStyle = FontStyle.Bold;
		tm.color = Color.white;
		var mr = go.GetComponent<MeshRenderer>();
		mr.material = fuente.material;
		mr.sortingOrder = 50;
		go.AddComponent<Flotar>().Subir(1.2f, 0.8f);
	}

	// ------------------------------------------------ Vidas, muerte y puntos de control

	public void PuntoDeControl(Vector3 posicion) {
		puntoControl = posicion;
	}

	//Daño recibido: el jugador ya restó la vida, aquí solo se decide si muere
	public void VidaPerdida(string motivo) {
		if (Juego.I.vidas <= 0) Muerte(motivo);
	}

	//Caer al vacío o al agua cuesta una vida y se reaparece en el último punto de control
	public void CaidaAlVacio() {
		if (Terminado) return;
		Juego.I.vidas--;
		if (Juego.I.vidas <= 0) {
			Muerte("Ciguín cayó al vacío.");
			return;
		}
		Juego.I.Sonar(Juego.I.sDano);
		Jugador.I.Reaparecer(puntoControl);
		MostrarMensaje("¡CUIDADO!", "Perdiste una vida. Te quedan " + Juego.I.vidas + ".", 1.8f);
	}

	//Sin vidas o sin tiempo: se muestra que el personaje murió y se regresa al primer nivel
	public void Muerte(string motivo) {
		if (Terminado) return;
		Terminado = true;
		StartCoroutine(SecuenciaMuerte(motivo));
	}

	IEnumerator SecuenciaMuerte(string motivo) {
		Juego.I.vidas = 0;
		ActualizarHUD();
		Jugador.I.Morir();
		Juego.I.DetenerMusica();
		Juego.I.Sonar(Juego.I.sMuerte);
		panelJefe.SetActive(false);
		MensajeFijo("¡CIGUÍN HA MUERTO!", motivo + "\n\nVolviendo al Nivel 1...", new Color(1f, 0.3f, 0.25f));
		yield return new WaitForSeconds(4f);
		Juego.I.ReiniciarPartida();
		SceneManager.LoadScene("Nivel1");
	}

	public void Completar() {
		if (Terminado) return;
		Terminado = true;
		StartCoroutine(SecuenciaCompletado());
	}

	IEnumerator SecuenciaCompletado() {
		Jugador.I.Celebrar();
		Juego.I.DetenerMusica();
		Juego.I.Sonar(Juego.I.sNivelCompletado);
		panelJefe.SetActive(false);
		bool ultimo = escenaSiguiente == "Victoria";
		MensajeFijo(ultimo ? "¡DERROTASTE AL GALIPOTE!" : "¡NIVEL " + numero + " COMPLETADO!",
			"Bono de tiempo: " + Mathf.CeilToInt(Tiempo) + " x 10", new Color(1f, 0.85f, 0.2f));

		//El tiempo que sobra se convierte en puntos
		yield return new WaitForSeconds(1f);
		while (Tiempo > 0f) {
			float paso = Mathf.Min(Tiempo, 3f);
			Tiempo -= paso;
			Juego.I.SumarPuntos(Mathf.RoundToInt(paso * 10f));
			ActualizarHUD();
			yield return null;
		}
		Tiempo = 0f;
		yield return new WaitForSeconds(2.2f);
		SceneManager.LoadScene(escenaSiguiente);
	}

	// ------------------------------------------------ Jefe

	public void EmpezarJefe() {
		Juego.I.Musica(musicaJefe, 0.6f);
		panelJefe.SetActive(true);
		MostrarMensaje("¡EL GALIPOTE!", "Sáltale encima para hacerle daño", 2.5f);
	}

	public void VidaJefe(float fraccion) {
		barraJefe.fillAmount = fraccion;
	}
}
