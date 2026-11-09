using UnityEngine;

//Espinas del cactus, bolas de fuego y piedras del jefe. Se destruyen al tocar el suelo.
[RequireComponent(typeof(Rigidbody2D))]
public class Proyectil : MonoBehaviour {

	public float vida = 5f;
	public float giro = 360f;
	public GameObject efectoImpacto;
	Rigidbody2D rb;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
	}

	public void Lanzar(Vector2 velocidad, float gravedad) {
		rb.gravityScale = gravedad;
		rb.linearVelocity = velocidad;
	}

	void Start() {
		Destroy(gameObject, vida);
	}

	void Update() {
		transform.Rotate(0f, 0f, -giro * Time.deltaTime * Mathf.Sign(rb.linearVelocity.x + 0.001f));
	}

	void OnTriggerEnter2D(Collider2D otro) {
		if (otro.gameObject.layer == LayerMask.NameToLayer("Suelo") && !otro.isTrigger) {
			if (efectoImpacto != null) Instantiate(efectoImpacto, transform.position, Quaternion.identity);
			Destroy(gameObject);
		} else if (otro.GetComponent<Jugador>() != null) {
			//Peligro ya hizo el daño en este mismo contacto
			Destroy(gameObject, 0.02f);
		}
	}
}
