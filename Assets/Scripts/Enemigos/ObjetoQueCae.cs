using System.Collections;
using UnityEngine;

//Obstáculo 4 - Coco (nivel 1 y 3) o estalactita (nivel 2): tiembla y cae cuando Ciguín pasa por debajo
public class ObjetoQueCae : MonoBehaviour {

	public float distanciaX = 1.8f;
	public float gravedad = 3.5f;
	public GameObject efectoImpacto;
	Rigidbody2D rb;
	bool cayendo;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Kinematic;
	}

	void Update() {
		if (cayendo) return;
		var j = Jugador.I;
		Vector3 p = j.transform.position;
		if (Mathf.Abs(p.x - transform.position.x) < distanciaX && p.y < transform.position.y && p.y > transform.position.y - 12f) {
			cayendo = true;
			StartCoroutine(Caer());
		}
	}

	IEnumerator Caer() {
		Vector3 inicio = transform.position;
		for (float t = 0f; t < 0.45f; t += Time.deltaTime) {
			transform.position = inicio + new Vector3(Mathf.Sin(t * 70f) * 0.06f, 0f, 0f);
			yield return null;
		}
		transform.position = inicio;
		Juego.I.Sonar(Juego.I.sCaida, 0.6f);
		rb.bodyType = RigidbodyType2D.Dynamic;
		rb.gravityScale = gravedad;
	}

	void OnTriggerEnter2D(Collider2D otro) {
		if (!cayendo) return;
		if (otro.gameObject.layer == LayerMask.NameToLayer("Suelo") && !otro.isTrigger && rb.linearVelocity.y < -1f) {
			if (efectoImpacto != null) Instantiate(efectoImpacto, transform.position, Quaternion.identity);
			Juego.I.Sonar(Juego.I.sBloque, 0.5f);
			Destroy(gameObject);
		}
	}
}
