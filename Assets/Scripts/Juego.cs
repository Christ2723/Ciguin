using UnityEngine;

//Datos que se conservan entre escenas (vidas, larimares, puntos) y todo el audio del juego.
//Vive en Resources/Juego.prefab y se crea solo la primera vez que alguien lo pide.
public class Juego : MonoBehaviour {

	public const int VIDAS_INICIALES = 3;
	public const int VIDAS_MAX = 5;
	public const int LARIMARES_POR_VIDA = 50;

	static Juego instancia;

	public static Juego I {
		get {
			if (instancia == null) {
				var prefab = Resources.Load<Juego>("Juego");
				Instantiate(prefab).name = "Juego";
			}
			return instancia;
		}
	}

	[Header("Estado de la partida")]
	public int vidas = VIDAS_INICIALES;
	public int larimares;
	public int puntos;

	[Header("Fuentes de audio")]
	public AudioSource fuenteMusica;
	public AudioSource fuenteEfectos;

	[Header("Música")]
	public AudioClip musicaMenu;
	public AudioClip musicaVictoria;

	[Header("Efectos")]
	public AudioClip sSalto;
	public AudioClip sLarimar;
	public AudioClip sAplastar;
	public AudioClip sDano;
	public AudioClip sBloque;
	public AudioClip sRomper;
	public AudioClip sAparece;
	public AudioClip sMango;
	public AudioClip sVidaExtra;
	public AudioClip sDisparo;
	public AudioClip sFuego;
	public AudioClip sGolpeJefe;
	public AudioClip sRugido;
	public AudioClip sTemblor;
	public AudioClip sMuerte;
	public AudioClip sNivelCompletado;
	public AudioClip sCheckpoint;
	public AudioClip sPausa;
	public AudioClip sCaida;
	public AudioClip sBoton;

	void Awake() {
		if (instancia != null && instancia != this) {
			Destroy(gameObject);
			return;
		}
		instancia = this;
		DontDestroyOnLoad(gameObject);
	}

	public static int MejorPuntaje {
		get => PlayerPrefs.GetInt("MejorPuntaje", 0);
		private set { PlayerPrefs.SetInt("MejorPuntaje", value); PlayerPrefs.Save(); }
	}

	public void GuardarMejorPuntaje() {
		if (puntos > MejorPuntaje) MejorPuntaje = puntos;
	}

	//Partida nueva: se usa al pulsar Jugar y cada vez que el personaje muere
	public void ReiniciarPartida() {
		GuardarMejorPuntaje();
		vidas = VIDAS_INICIALES;
		larimares = 0;
		puntos = 0;
	}

	public void SumarPuntos(int cantidad) {
		puntos += cantidad;
	}

	public void AgregarLarimar() {
		larimares++;
		puntos += 100;
		Sonar(sLarimar, 0.7f);
		if (larimares >= LARIMARES_POR_VIDA) {
			larimares -= LARIMARES_POR_VIDA;
			GanarVida();
		}
	}

	public void GanarVida() {
		if (vidas < VIDAS_MAX) {
			vidas++;
			Sonar(sVidaExtra);
		} else {
			puntos += 1000;
			Sonar(sMango);
		}
	}

	// ------------------------------------------------ Audio

	public void Sonar(AudioClip clip, float volumen = 1f) {
		if (clip != null) fuenteEfectos.PlayOneShot(clip, volumen);
	}

	public void Musica(AudioClip clip, float volumen = 0.55f) {
		if (fuenteMusica.clip == clip && fuenteMusica.isPlaying) return;
		fuenteMusica.clip = clip;
		fuenteMusica.volume = volumen;
		fuenteMusica.loop = true;
		if (clip != null) fuenteMusica.Play();
	}

	public void DetenerMusica() {
		fuenteMusica.Stop();
		fuenteMusica.clip = null;
	}

	public void PausarMusica(bool pausar) {
		if (pausar) fuenteMusica.Pause(); else fuenteMusica.UnPause();
	}
}
