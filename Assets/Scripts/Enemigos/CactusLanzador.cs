using UnityEngine;

//Enemigo 3 - Cactus lanzador: no se puede aplastar (tiene espinas) y lanza espinas en arco hacia Ciguín
public class CactusLanzador : Enemigo {

	public Sprite spriteDisparo;
	public Proyectil prefabEspina;
	public float intervalo = 2.4f;
	public float alcance = 11f;
	float reloj;

	protected override void Awake() {
		base.Awake();
		reloj = Random.Range(0.5f, intervalo);
	}

	protected override void Update() {
		if (!vivo || !Activo) return;
		var j = Jugador.I;
		if (j.Muerto) return;
		grafico.flipX = j.transform.position.x > transform.position.x;

		reloj -= Time.deltaTime;
		grafico.sprite = reloj < 0.35f ? spriteDisparo : frames[(int)(Time.time * 2f) % frames.Length];
		if (reloj <= 0f) {
			reloj = intervalo;
			if (Mathf.Abs(j.transform.position.x - transform.position.x) < alcance) Disparar(j.transform.position);
		}
	}

	void Disparar(Vector3 hacia) {
		float dx = hacia.x - transform.position.x;
		var espina = Instantiate(prefabEspina, transform.position + Vector3.up * 0.5f, Quaternion.identity);
		//Arco: tarda ~1.1 s en llegar a donde estaba el jugador
		float tiempo = 1.1f;
		float vx = Mathf.Clamp(dx / tiempo, -7f, 7f);
		espina.Lanzar(new Vector2(vx, 7.5f), 1.6f);
		Juego.I.Sonar(Juego.I.sDisparo, 0.5f);
	}
}
