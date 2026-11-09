using UnityEngine;

//Cámara que sigue a Ciguín en horizontal, sin salirse de los límites del nivel
public class CamaraSeguir : MonoBehaviour {

	public static CamaraSeguir I;

	public Transform objetivo;
	public float minX = 13.4f;
	public float maxX = 186.6f;
	public float altura = 7.5f;
	public float adelanto = 2f;
	public float suavizado = 6f;

	float sacudida;
	float fuerzaSacudida;
	float adelantoActual;

	void Awake() {
		I = this;
	}

	void Start() {
		if (objetivo != null) transform.position = new Vector3(Mathf.Clamp(objetivo.position.x, minX, maxX), altura, -10f);
	}

	void LateUpdate() {
		if (objetivo == null) return;
		var rb = objetivo.GetComponent<Rigidbody2D>();
		float vx = rb != null ? rb.linearVelocity.x : 0f;
		if (Mathf.Abs(vx) > 1f) adelantoActual = Mathf.MoveTowards(adelantoActual, Mathf.Sign(vx) * adelanto, 4f * Time.deltaTime);

		float x = Mathf.Lerp(transform.position.x, objetivo.position.x + adelantoActual, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
		x = Mathf.Clamp(x, minX, maxX);
		Vector3 pos = new Vector3(x, altura, -10f);

		if (sacudida > 0f) {
			sacudida -= Time.deltaTime;
			pos += (Vector3)(Random.insideUnitCircle * fuerzaSacudida);
		}
		transform.position = pos;
	}

	public void Sacudir(float segundos, float fuerza) {
		sacudida = segundos;
		fuerzaSacudida = fuerza;
	}
}
