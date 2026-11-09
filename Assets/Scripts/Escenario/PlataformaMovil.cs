using UnityEngine;

//Plataforma de madera que va y viene entre dos puntos. Ciguín se mueve con ella.
[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour {

	public Vector2 desplazamiento = new Vector2(4f, 0f);
	public float periodo = 4f;
	public Vector2 Velocidad { get; private set; }

	Rigidbody2D rb;
	Vector2 inicio;
	float t;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Kinematic;
		rb.interpolation = RigidbodyInterpolation2D.Interpolate;
		inicio = rb.position;
	}

	void FixedUpdate() {
		t += Time.fixedDeltaTime;
		float k = (1f - Mathf.Cos(t / periodo * 2f * Mathf.PI)) * 0.5f;
		Vector2 nueva = inicio + desplazamiento * k;
		Velocidad = (nueva - rb.position) / Time.fixedDeltaTime;
		rb.MovePosition(nueva);
	}
}
