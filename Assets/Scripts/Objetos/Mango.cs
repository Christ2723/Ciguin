using System.Collections;
using UnityEngine;

//Mango: sale de un bloque sorpresa, se desliza por el suelo y da una vida (corazón) extra
public class Mango : MonoBehaviour {

	public float velocidad = 2.5f;
	public LayerMask mascaraSuelo;
	Rigidbody2D rb;
	Collider2D cuerpo;
	float direccion = 1f;
	bool saliendo = true;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		cuerpo = GetComponent<Collider2D>();
	}

	IEnumerator Start() {
		//Sube saliendo del bloque
		rb.bodyType = RigidbodyType2D.Kinematic;
		cuerpo.enabled = false;
		Vector3 inicio = transform.position;
		for (float t = 0f; t < 0.6f; t += Time.deltaTime) {
			transform.position = inicio + Vector3.up * (t / 0.6f);
			yield return null;
		}
		transform.position = inicio + Vector3.up;
		cuerpo.enabled = true;
		rb.bodyType = RigidbodyType2D.Dynamic;
		saliendo = false;
		direccion = Jugador.I.transform.position.x > transform.position.x ? -1f : 1f;
	}

	void FixedUpdate() {
		if (saliendo) return;
		if (Physics2D.Raycast(rb.position, new Vector2(direccion, 0f), 0.5f, mascaraSuelo)) direccion = -direccion;
		rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
		if (transform.position.y < -3f) Destroy(gameObject);
	}

	//La llama la zona de recogida (hija en la capa Recogible)
	public void Recoger() {
		if (saliendo) return;
		Juego.I.Sonar(Juego.I.sMango);
		Juego.I.GanarVida();
		Nivel.I.PuntosFlotantes(1000, transform.position);
		Destroy(gameObject);
	}
}
