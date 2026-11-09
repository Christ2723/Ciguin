using System.Collections;
using UnityEngine;

//Base de todos los enemigos. El cuerpo (capa Enemigos) choca con el suelo
//y la "zona de golpe" hija (capa Golpe) detecta al jugador.
public class Enemigo : MonoBehaviour {

	[Header("Enemigo")]
	public bool aplastable = true;
	public int puntos = 200;
	public string motivo = "Un enemigo acabó con Ciguín.";
	public SpriteRenderer grafico;
	public Sprite[] frames;
	public float fpsAnimacion = 6f;
	public Sprite spriteAplastado;

	protected Rigidbody2D rb;
	protected bool vivo = true;
	protected static Camera camara;

	//Los enemigos solo se mueven cuando están cerca de la cámara
	protected bool Activo {
		get {
			if (camara == null) camara = Camera.main;
			return Mathf.Abs(transform.position.x - camara.transform.position.x) < 17f;
		}
	}

	protected virtual void Awake() {
		rb = GetComponent<Rigidbody2D>();
	}

	protected virtual void Update() {
		if (vivo && frames != null && frames.Length > 0 && Activo) {
			grafico.sprite = frames[(int)(Time.time * fpsAnimacion) % frames.Length];
		}
	}

	//La llama ZonaGolpe cuando el jugador toca al enemigo
	public virtual void TocarJugador(Jugador j, Collider2D zona) {
		if (!vivo) return;
		if (aplastable && j.CaeSobre(zona)) Aplastar(j);
		else j.Danar(transform.position, motivo);
	}

	protected virtual void Aplastar(Jugador j) {
		vivo = false;
		j.Rebotar();
		Juego.I.Sonar(Juego.I.sAplastar);
		Nivel.I.PuntosFlotantes(puntos, transform.position);
		DesactivarColisiones();
		if (rb != null) {
			rb.linearVelocity = Vector2.zero;
			rb.bodyType = RigidbodyType2D.Kinematic;
		}
		if (spriteAplastado != null) grafico.sprite = spriteAplastado;
		else grafico.transform.localScale = new Vector3(1.2f, 0.4f, 1f);
		StartCoroutine(Desaparecer(0.6f));
	}

	//Golpeado desde abajo por un bloque: sale volando boca abajo
	public virtual void Derribar() {
		if (!vivo) return;
		vivo = false;
		Juego.I.Sonar(Juego.I.sAplastar);
		Nivel.I.PuntosFlotantes(puntos, transform.position);
		DesactivarColisiones();
		grafico.flipY = true;
		if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Dynamic;
		rb.gravityScale = 3f;
		rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), 10f);
		grafico.sortingOrder = 40;
		StartCoroutine(Desaparecer(2f));
	}

	protected void DesactivarColisiones() {
		foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
	}

	IEnumerator Desaparecer(float segundos) {
		yield return new WaitForSeconds(segundos);
		Destroy(gameObject);
	}
}
