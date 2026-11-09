using UnityEngine;

//Enemigo 2 - Murciélago: vuela en ondas y se lanza en picada cuando Ciguín pasa cerca
public class Murcielago : Enemigo {

	public float rangoX = 2.5f;
	public float velocidadPicada = 7f;

	enum Estado { Patrulla, Picada, Regreso }
	Estado estado;
	Vector2 origen;
	Vector2 objetivo;
	float t;
	float espera;

	protected override void Awake() {
		base.Awake();
		origen = transform.position;
		t = Random.Range(0f, 6f);
	}

	void FixedUpdate() {
		if (!vivo || !Activo) return;
		var j = Jugador.I;
		Vector2 pos = rb.position;
		espera -= Time.fixedDeltaTime;

		switch (estado) {
			case Estado.Patrulla:
				t += Time.fixedDeltaTime;
				Vector2 destino = origen + new Vector2(Mathf.Sin(t * 0.9f) * rangoX, Mathf.Sin(t * 2.7f) * 0.6f);
				rb.MovePosition(Vector2.MoveTowards(pos, destino, 6f * Time.fixedDeltaTime));
				grafico.flipX = Mathf.Cos(t * 0.9f) > 0f;
				if (espera <= 0f && !j.Muerto && Mathf.Abs(j.transform.position.x - pos.x) < 4.5f &&
					j.transform.position.y < pos.y - 0.5f && j.transform.position.y > pos.y - 7f) {
					objetivo = j.transform.position;
					estado = Estado.Picada;
				}
				break;
			case Estado.Picada:
				rb.MovePosition(Vector2.MoveTowards(pos, objetivo, velocidadPicada * Time.fixedDeltaTime));
				grafico.flipX = objetivo.x > pos.x;
				if (Vector2.Distance(pos, objetivo) < 0.1f) estado = Estado.Regreso;
				break;
			case Estado.Regreso:
				Vector2 vuelta = origen + new Vector2(Mathf.Sin(t * 0.9f) * rangoX, 0f);
				rb.MovePosition(Vector2.MoveTowards(pos, vuelta, 4f * Time.fixedDeltaTime));
				if (Vector2.Distance(pos, vuelta) < 0.1f) {
					estado = Estado.Patrulla;
					espera = 2.5f;
				}
				break;
		}
	}
}
