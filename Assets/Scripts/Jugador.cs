using UnityEngine;

//Ciguín: se mueve, salta (más alto si se mantiene el botón), rebota sobre los enemigos
//y golpea bloques con la cabeza. Las animaciones se hacen cambiando sprites.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Jugador : MonoBehaviour {

	public static Jugador I;

	[Header("Movimiento")]
	public float velocidad = 7f;
	public float velocidadCorrer = 9.5f;
	public float aceleracion = 55f;
	public float frenado = 70f;
	public float fuerzaSalto = 17.5f;
	public float fuerzaRebote = 13f;
	public float gravedadSubida = 4f;
	public float gravedadCaida = 6f;
	public float caidaMaxima = 20f;
	public LayerMask mascaraSuelo;

	[Header("Sprites")]
	public SpriteRenderer grafico;
	public Sprite[] quieto;
	public Sprite[] caminar;
	public Sprite saltar;
	public Sprite caer;
	public Sprite herido;
	public Sprite celebrar;

	public bool EnSuelo { get; private set; }
	public Collider2D SueloActual { get; private set; }
	public bool Muerto { get; private set; }
	public bool Invulnerable => invulnerable > 0f;

	Rigidbody2D rb;
	BoxCollider2D col;
	float velX;
	float entradaX;
	bool correr;
	float coyote;
	float bufferSalto;
	bool saltoCortado;
	float invulnerable;
	float tiempoAnim;
	float tiempoParpadeo;
	bool controlable = true;
	bool celebrando;
	float ultimoGolpeBloque;

	void Awake() {
		I = this;
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<BoxCollider2D>();
		rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
	}

	void Update() {

		if (Muerto || Time.timeScale == 0f) return;

		if (controlable) {
			entradaX = Entrada.Horizontal;
			correr = Entrada.Correr;
			if (Entrada.SaltoAbajo) bufferSalto = 0.12f;
		}

		if (invulnerable > 0f) {
			invulnerable -= Time.deltaTime;
			grafico.enabled = invulnerable <= 0f || (Time.time * 14f) % 2f > 0.6f;
		}

		Animar();

		if (transform.position.y < -3f && Nivel.I != null) Nivel.I.CaidaAlVacio();
	}

	static bool BotonSaltoMantenido() => Entrada.SaltoMantenido;

	void FixedUpdate() {

		if (Muerto) return;
		float dt = Time.fixedDeltaTime;

		//Comprobación de suelo con una caja delgada bajo los pies
		Bounds b = col.bounds;
		var golpe = Physics2D.BoxCast(new Vector2(b.center.x, b.min.y + 0.06f), new Vector2(b.size.x * 0.85f, 0.1f), 0f,
			Vector2.down, 0.1f, mascaraSuelo);
		Vector2 velSuelo = Vector2.zero;
		var plataforma = golpe.collider != null ? golpe.collider.GetComponentInParent<PlataformaMovil>() : null;
		if (plataforma != null) velSuelo = plataforma.Velocidad;
		EnSuelo = golpe.collider != null && rb.linearVelocity.y <= velSuelo.y + 0.1f;
		SueloActual = EnSuelo ? golpe.collider : null;

		if (EnSuelo) coyote = 0.1f; else coyote -= dt;
		bufferSalto -= dt;

		//Movimiento horizontal con aceleración
		float objetivo = entradaX * (correr ? velocidadCorrer : velocidad);
		float acel = Mathf.Abs(objetivo) > 0.01f && Mathf.Sign(objetivo) == Mathf.Sign(velX) ? aceleracion : frenado;
		if (!EnSuelo) acel *= 0.75f;
		velX = Mathf.MoveTowards(velX, objetivo, acel * dt);

		float vy = rb.linearVelocity.y;
		if (EnSuelo && plataforma != null) vy = Mathf.Min(vy, velSuelo.y);

		//Salto con tiempo de coyote y buffer
		if (bufferSalto > 0f && coyote > 0f && controlable) {
			vy = fuerzaSalto + Mathf.Abs(velX) * 0.25f;
			bufferSalto = 0f;
			coyote = 0f;
			saltoCortado = false;
			EnSuelo = false;
			Juego.I.Sonar(Juego.I.sSalto, 0.6f);
		}

		//Salto variable: si suelta el botón mientras sube, sube menos
		if (!saltoCortado && vy > 0f && !BotonSaltoMantenido() && !EnSuelo) {
			vy *= 0.5f;
			saltoCortado = true;
		}

		rb.gravityScale = vy > 0f ? gravedadSubida : gravedadCaida;
		vy = Mathf.Max(vy, -caidaMaxima);

		rb.linearVelocity = new Vector2(velX + (EnSuelo ? velSuelo.x : 0f), vy);

		//No dejar que salga por el borde izquierdo o derecho del nivel
		if (Nivel.I != null) {
			float x = Mathf.Clamp(rb.position.x, 0.5f, Nivel.I.anchoNivel - 0.5f);
			if (x != rb.position.x) {
				rb.position = new Vector2(x, rb.position.y);
				velX = 0f;
			}
		}
	}

	void Animar() {

		if (celebrando) {
			grafico.sprite = celebrar;
			return;
		}
		if (entradaX != 0f && controlable) grafico.flipX = entradaX < 0f;

		if (invulnerable > 1.2f) {
			grafico.sprite = herido;
		} else if (!EnSuelo) {
			grafico.sprite = rb.linearVelocity.y > 0f ? saltar : caer;
		} else if (Mathf.Abs(velX) > 0.4f) {
			tiempoAnim += Time.deltaTime * Mathf.Lerp(6f, 14f, Mathf.Abs(velX) / velocidadCorrer);
			grafico.sprite = caminar[(int)tiempoAnim % caminar.Length];
		} else {
			//Parpadea de vez en cuando
			tiempoParpadeo += Time.deltaTime;
			grafico.sprite = (tiempoParpadeo % 3.2f) > 3.05f ? quieto[1] : quieto[0];
		}
	}

	//Golpear bloques con la cabeza
	void OnCollisionEnter2D(Collision2D c) {
		if (Muerto) return;
		var bloque = c.collider.GetComponent<Bloque>();
		if (bloque == null) return;
		for (int i = 0; i < c.contactCount; i++) {
			if (c.GetContact(i).normal.y < -0.6f) {
				if (Time.time - ultimoGolpeBloque < 0.05f) return;
				//Si toca dos bloques a la vez, solo golpea el que está más centrado
				if (Mathf.Abs(bloque.transform.position.x - transform.position.x) > 0.75f) return;
				ultimoGolpeBloque = Time.time;
				bloque.Golpear();
				return;
			}
		}
	}

	// ------------------------------------------------ Lo que llaman los enemigos y el nivel

	//¿Viene cayendo encima de esta zona? (para aplastar enemigos)
	public bool CaeSobre(Collider2D zona) {
		//Se usa la posición de los pies en el paso anterior: al caer rápido el contacto se detecta un poco tarde
		float piesAntes = col.bounds.min.y - Mathf.Min(rb.linearVelocity.y, 0f) * Time.fixedDeltaTime * 1.5f;
		return rb.linearVelocity.y < 2f && piesAntes > zona.bounds.center.y;
	}

	public void Rebotar(float multiplicador = 1f) {
		float fuerza = fuerzaRebote * multiplicador;
		if (BotonSaltoMantenido()) fuerza *= 1.3f;
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerza);
		saltoCortado = false;
	}

	public void Danar(Vector3 desde, string motivo) {
		if (Muerto || invulnerable > 0f || !controlable) return;
		Juego.I.vidas--;
		if (Juego.I.vidas > 0) {
			Juego.I.Sonar(Juego.I.sDano);
			invulnerable = 1.8f;
			float lado = transform.position.x >= desde.x ? 1f : -1f;
			velX = lado * 7f;
			rb.linearVelocity = new Vector2(velX, 9f);
		}
		Nivel.I.VidaPerdida(motivo);
	}

	public void Reaparecer(Vector3 punto) {
		rb.linearVelocity = Vector2.zero;
		velX = 0f;
		transform.position = punto + Vector3.up * 0.5f;
		invulnerable = 2f;
	}

	public void Morir() {
		if (Muerto) return;
		Muerto = true;
		col.enabled = false;
		grafico.enabled = true;
		grafico.sprite = herido;
		grafico.sortingOrder = 60;
		rb.gravityScale = 4f;
		rb.linearVelocity = new Vector2(0f, 15f);
	}

	public void Celebrar() {
		controlable = false;
		entradaX = 0f;
		velX = 0f;
		celebrando = true;
		invulnerable = 0f;
		grafico.enabled = true;
		rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y, 8f));
	}
}
