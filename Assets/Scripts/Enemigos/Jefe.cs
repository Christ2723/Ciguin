using System.Collections;
using UnityEngine;

//Jefe del nivel 3 - El Galipote (figura del folclor dominicano que cambia de forma).
//Camina hacia Ciguín, salta haciendo temblar el suelo, embiste y lanza bolas de fuego.
//Solo se le hace daño saltándole encima; a mitad de vida se enfurece y es más rápido.
public class Jefe : Enemigo {

	[Header("Jefe")]
	public int vidaMaxima = 6;
	public Sprite spriteAtaque;
	public Sprite spriteHerido;
	public Proyectil prefabFuego;
	public Proyectil prefabPiedra;
	public GameObject efectoExplosion;
	public float limiteIzquierdo;
	public float limiteDerecho;
	public LayerMask mascaraSuelo;

	int vida;
	bool despierto;
	bool invulnerable;
	bool atacando;
	bool enSuelo;
	bool Furioso => vida <= vidaMaxima / 2;

	protected override void Awake() {
		base.Awake();
		vida = vidaMaxima;
		rb.bodyType = RigidbodyType2D.Kinematic;
	}

	public void Despertar() {
		if (despierto) return;
		despierto = true;
		rb.bodyType = RigidbodyType2D.Dynamic;
		Juego.I.Sonar(Juego.I.sRugido);
		Nivel.I.EmpezarJefe();
		Nivel.I.VidaJefe(1f);
		StartCoroutine(Comportamiento());
	}

	protected override void Update() {
		if (!vivo) return;
		var b = GetComponent<Collider2D>().bounds;
		enSuelo = Physics2D.BoxCast(new Vector2(b.center.x, b.min.y + 0.05f), new Vector2(b.size.x * 0.8f, 0.1f), 0f,
			Vector2.down, 0.1f, mascaraSuelo).collider != null;

		if (invulnerable) {
			grafico.sprite = spriteHerido;
			grafico.color = (Time.time * 12f) % 2f > 1f ? Color.white : new Color(1f, 0.5f, 0.5f);
		} else if (atacando) {
			grafico.sprite = spriteAtaque;
			grafico.color = Furioso ? new Color(1f, 0.75f, 0.7f) : Color.white;
		} else {
			grafico.sprite = frames[(int)(Time.time * (Furioso ? 9f : 6f)) % frames.Length];
			grafico.color = Furioso ? new Color(1f, 0.75f, 0.7f) : Color.white;
		}
		if (despierto && !Jugador.I.Muerto && !atacando) grafico.flipX = Jugador.I.transform.position.x > transform.position.x;
	}

	float DireccionAlJugador => Mathf.Sign(Jugador.I.transform.position.x - transform.position.x);

	IEnumerator Comportamiento() {
		yield return new WaitForSeconds(1.5f);
		int ataque = 0;
		while (vivo) {

			//Camina hacia el jugador
			float caminar = Furioso ? Random.Range(0.8f, 1.4f) : Random.Range(1.4f, 2.2f);
			for (float t = 0f; t < caminar && vivo; t += Time.deltaTime) {
				rb.linearVelocity = new Vector2(DireccionAlJugador * (Furioso ? 3.8f : 2.6f), rb.linearVelocity.y);
				yield return null;
			}
			if (!vivo) yield break;
			rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

			switch (ataque % 3) {
				case 0: yield return Saltar(); break;
				case 1: yield return LanzarFuego(); break;
				case 2: yield return Embestir(); break;
			}
			ataque += Random.Range(1, 3);
			yield return new WaitForSeconds(Furioso ? 0.35f : 0.7f);
		}
	}

	IEnumerator Saltar() {
		atacando = true;
		yield return new WaitForSeconds(0.3f);
		float dx = Jugador.I.transform.position.x - transform.position.x;
		rb.linearVelocity = new Vector2(Mathf.Clamp(dx / 1.3f, -8f, 8f), 17f);
		yield return new WaitForSeconds(0.3f);
		while (vivo && !enSuelo) yield return null;
		if (!vivo) yield break;
		rb.linearVelocity = Vector2.zero;
		atacando = false;

		//Al caer tiembla el suelo y caen piedras del cielo
		Juego.I.Sonar(Juego.I.sTemblor);
		CamaraSeguir.I.Sacudir(0.5f, 0.35f);
		int piedras = Furioso ? 4 : 2;
		for (int i = 0; i < piedras; i++) {
			float x = Random.Range(limiteIzquierdo + 1f, limiteDerecho - 1f);
			var p = Instantiate(prefabPiedra, new Vector3(x, 14.5f, 0f), Quaternion.identity);
			p.Lanzar(Vector2.zero, 1.2f);
		}
	}

	IEnumerator LanzarFuego() {
		atacando = true;
		int bolas = Furioso ? 5 : 3;
		for (int i = 0; i < bolas && vivo; i++) {
			yield return new WaitForSeconds(0.3f);
			var f = Instantiate(prefabFuego, transform.position + new Vector3(DireccionAlJugador * 1f, 0.4f, 0f), Quaternion.identity);
			float dx = Jugador.I.transform.position.x - transform.position.x;
			f.Lanzar(new Vector2(Mathf.Clamp(dx / 1.1f, -9f, 9f) + Random.Range(-1.5f, 1.5f), Random.Range(6f, 9f)), 1.6f);
			Juego.I.Sonar(Juego.I.sFuego, 0.7f);
		}
		yield return new WaitForSeconds(0.3f);
		atacando = false;
	}

	IEnumerator Embestir() {
		atacando = true;
		float dir = DireccionAlJugador;
		grafico.flipX = dir > 0f;
		//Aviso: se agacha y tiembla
		Vector3 escala = grafico.transform.localScale;
		for (float t = 0f; t < 0.5f; t += Time.deltaTime) {
			grafico.transform.localScale = new Vector3(escala.x * 1.08f, escala.y * 0.9f, 1f);
			grafico.transform.localPosition = new Vector3(Mathf.Sin(t * 60f) * 0.05f, -0.1f, 0f);
			yield return null;
		}
		grafico.transform.localScale = escala;
		grafico.transform.localPosition = Vector3.zero;
		Juego.I.Sonar(Juego.I.sRugido, 0.6f);
		for (float t = 0f; t < 1.4f && vivo; t += Time.deltaTime) {
			rb.linearVelocity = new Vector2(dir * (Furioso ? 11f : 8.5f), rb.linearVelocity.y);
			float x = transform.position.x;
			if ((dir < 0f && x < limiteIzquierdo + 1.2f) || (dir > 0f && x > limiteDerecho - 1.2f)) {
				CamaraSeguir.I.Sacudir(0.3f, 0.25f);
				Juego.I.Sonar(Juego.I.sTemblor, 0.6f);
				break;
			}
			yield return null;
		}
		rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
		atacando = false;
	}

	public override void TocarJugador(Jugador j, Collider2D zona) {
		if (!vivo || !despierto) return;
		if (j.CaeSobre(zona)) {
			if (invulnerable) {
				j.Rebotar(0.8f);
				return;
			}
			j.Rebotar(1.25f);
			Herir();
		} else if (!invulnerable) {
			j.Danar(transform.position, "El Galipote derrotó a Ciguín.");
		}
	}

	void Herir() {
		vida--;
		Juego.I.Sonar(Juego.I.sGolpeJefe);
		Nivel.I.VidaJefe(vida / (float)vidaMaxima);
		CamaraSeguir.I.Sacudir(0.25f, 0.2f);
		if (vida <= 0) {
			vivo = false;
			StopAllCoroutines();
			StartCoroutine(SecuenciaDerrota());
			return;
		}
		if (vida == vidaMaxima / 2) Juego.I.Sonar(Juego.I.sRugido);
		StartCoroutine(Invulnerabilidad());
	}

	IEnumerator Invulnerabilidad() {
		invulnerable = true;
		yield return new WaitForSeconds(1.1f);
		invulnerable = false;
	}

	IEnumerator SecuenciaDerrota() {
		rb.linearVelocity = Vector2.zero;
		rb.bodyType = RigidbodyType2D.Kinematic;
		grafico.sprite = spriteHerido;
		Nivel.I.PuntosFlotantes(5000, transform.position + Vector3.up);
		Juego.I.DetenerMusica();
		//Explosiones mientras tiembla
		for (int i = 0; i < 8; i++) {
			grafico.color = i % 2 == 0 ? Color.white : new Color(1f, 0.4f, 0.4f);
			Instantiate(efectoExplosion, transform.position + (Vector3)Random.insideUnitCircle * 1.2f, Quaternion.identity);
			Juego.I.Sonar(Juego.I.sRomper, 0.7f);
			CamaraSeguir.I.Sacudir(0.2f, 0.2f);
			yield return new WaitForSeconds(0.2f);
		}
		DesactivarColisiones();
		grafico.flipY = true;
		rb.bodyType = RigidbodyType2D.Dynamic;
		rb.gravityScale = 3f;
		rb.linearVelocity = new Vector2(0f, 8f);
		yield return new WaitForSeconds(1f);
		Nivel.I.Completar();
	}
}
